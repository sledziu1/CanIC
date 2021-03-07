/*
 * sender.c
 *
 *  Created on: 06.12.2018
 */

#include "CANSPI.h"

void updateOutsideTemp(int16_t temp);

#define F11BIT dSTANDARD_CAN_MSG_ID_2_0B

#define ALTERNATIVE_GEARUPDN 1

/* local proto */
void btnHandle(void);

typedef struct
{
	uCAN_MSG message;
	const uint16_t sendInterval;
	volatile int16_t currentInterval;
} FRAME;

static FRAME speedRpmFrame =    {{.frame.idType = F11BIT, .frame.id = 0x2,   .frame.dlc=8}, 100,  0};
static FRAME carErrorFrame =    {{.frame.idType = F11BIT, .frame.id = 0x3,   .frame.dlc=7}, 200,  20};
static FRAME afsLightFrame =    {{.frame.idType = F11BIT, .frame.id = 0x1A7, .frame.dlc=4}, 200,  85};
static FRAME outsideTempFrame = {{.frame.idType = F11BIT, .frame.id = 0x15,  .frame.dlc=2}, 500,  50};
static FRAME hoodBrakeFrame =   {{.frame.idType = F11BIT, .frame.id = 0x1AE, .frame.dlc=1}, 200,  150};
static FRAME doorsFrame =       {{.frame.idType = F11BIT, .frame.id = 0x13,  .frame.dlc=3}, 500,  150};
static FRAME airbagFrame =      {{.frame.idType = F11BIT, .frame.id = 0x8,   .frame.dlc=4}, 500,  250};
static FRAME lightFrame =       {{.frame.idType = F11BIT, .frame.id = 0x6,   .frame.dlc=2}, 200,  10};
static FRAME engineTempFrame =  {{.frame.idType = F11BIT, .frame.id = 0x2C,  .frame.dlc=2}, 500,  350};
static FRAME carInfoFrame =     {{.frame.idType = F11BIT, .frame.id = 0x4,   .frame.dlc=6}, 1000, 250};


static FRAME* message_array[] = {
		&speedRpmFrame,
		&carErrorFrame,
		&afsLightFrame,
		&outsideTempFrame,
		&hoodBrakeFrame,
		&doorsFrame,
		&airbagFrame,
		&lightFrame,
		&engineTempFrame,
		&carInfoFrame,
};

static volatile uint16_t buttonTime=0;

typedef enum Gear_Enum_
{
	g0=0,
	g1=1,
	g2=2,
	g3=3,
	g4=4,
	g5=5,
	g6=6,
	D1=11,
	D2=12,
	D3=13,
	D4=14,
	D5=15,
	D6=16,
	P=21,
	R=22,
	N=23,
	D=24,
	Ds=25,
	A=26,
	Dx=27,
	gUP=28,
	gDN=29,
	unknown=30
} Gear_Enum;

void LoadCanFramesDefaultData(void)
{
	updateOutsideTemp(20);

	/* remove avg consumption */
	afsLightFrame.message.frame.data0=0xFF;
	afsLightFrame.message.frame.data1=0xFF;

	/* perfect water temp */
	engineTempFrame.message.frame.data1=0x92;
}

void run(void)
{
	for(uint8_t i=0;i<sizeof(message_array)/sizeof(message_array[0]);i++)
	{
		if(message_array[i]->sendInterval<=message_array[i]->currentInterval)
		{
			//__disable_irq();
			/* try to send data */
			uint8_t ret = CANSPI_Transmit(&(message_array[i]->message));
			if(ret==1) /* send successfully */
			{
				/* consider critical section for decrementation - seems useless */
				message_array[i]->currentInterval-=message_array[i]->sendInterval;
			}
			//__enable_irq();
		}
	}
}

void handleMsTick(void)
{
	for(uint8_t i=0;i<sizeof(message_array)/sizeof(message_array[0]);i++)
	{
		message_array[i]->currentInterval++;
	}

	btnHandle();
}

void updateSpeed(uint32_t speed)
{
	uint32_t bitspeed=3.83315f * speed;

	speedRpmFrame.message.frame.data2=bitspeed>>3 & 0xFF;
	speedRpmFrame.message.frame.data3=bitspeed<<5 & 0xFF;
}


volatile float rpmScale=0.06184472f;
volatile float rpmOffs=1.813664596f;
void updateRpm(uint32_t rpm)
{
	float rpmF = rpmScale * rpm +rpmOffs;
	if(rpmF < 0.0f)
	{
		rpmF=0.0f;
	}
	uint32_t bitrpm=rpmF;

	speedRpmFrame.message.frame.data0=bitrpm>>4 & 0xFF;
	speedRpmFrame.message.frame.data1=bitrpm<<4 & 0xFF;
}

void updateGearUp(uint32_t v)
{
#if ALTERNATIVE_GEARUPDN==0
	if(v==1)
	{
		carInfoFrame.message.frame.data3 |= 1<<4;
	}
	else
	{
		carInfoFrame.message.frame.data3 &= ~(1<<4);
	}

	//carInfoFrame.currentInterval=carInfoFrame.sendInterval;
#else
	if(v==1)
	{
		carErrorFrame.message.frame.data5 = 0x18;
	}
	else
	{
		carErrorFrame.message.frame.data5 = 0x0;
	}

	//carErrorFrame.currentInterval=carErrorFrame.sendInterval;
#endif
}

uint8_t charToDec(char ch)
{
	uint8_t retval=0u;

	if(48<=ch && ch <=57)
	{
		retval=ch-48;
	}

	return retval;
}

void updateGearNew(char v)
{

}

void updateGear(uint32_t v)
{
	switch ((Gear_Enum)v)
	{
	case N:
		carErrorFrame.message.frame.data4 = 0x4E;
		break;

	case R:
		carErrorFrame.message.frame.data4 = 0x52;
		break;

	case D:
		carErrorFrame.message.frame.data4 = 0x44; /* D */
		break;

	case g1:
	case g2:
	case g3:
	case g4:
	case g5:
	case g6:
		carErrorFrame.message.frame.data4 = 0x30 | v; /* bez D */
		break;

	case D1:
	case D2:
	case D3:
	case D4:
	case D5:
	case D6:
		carErrorFrame.message.frame.data4 = 0x80 | (v-10); /* z D */
		break;

	default:
		carErrorFrame.message.frame.data4 = 0x0;
		break;
	}

	//carErrorFrame.currentInterval=carErrorFrame.sendInterval;
}

/* TODO na inty */
volatile float outsideTempScale=2;
volatile float outsideTempOffs=80;
void updateOutsideTemp(int16_t temp)
{
	static int16_t oldtemp;

	float tempF = outsideTempScale * temp +outsideTempOffs;

	uint32_t bitTemp=tempF;

	outsideTempFrame.message.frame.data0=bitTemp & 0xFF;

	if(oldtemp!=temp)
	{
		outsideTempFrame.currentInterval-=150;
		oldtemp=temp;
	}
}

void updateFuel(int16_t fuel)
{
	if(fuel<0)
		fuel=0;
	else if(fuel>100)
		fuel=100;

	float x = fuel;

	float c = 6.9457142857142230e+001;
	float b = 2.9683428571428578e+001;
	float a = 5.0308571428571425e-001;

	float y = a*x*x;
	y+=b*x;
	y+=c;

	uint16_t fixedFuel=y;

	TIM1->CCR3 = fixedFuel;

	/* https://arachnoid.com/polysolve/
	0 1
	25 1200
	50 3000
	75 4800
	100 8200 */
}

void btnPress(uint8_t type)
{
	if(buttonTime==0)
	{
		if(type==1) //long
		{
			buttonTime=1200;
		}
		else //short
		{
			buttonTime=50;
		}

		HAL_GPIO_WritePin(GPIOB, GPIO_PIN_14, GPIO_PIN_SET);
	}
}

void setBitVal(uint8_t* item, uint8_t bitPos, uint8_t newVal)
{
	if(newVal == 0 || newVal == '0')
	{
		(*item) &= ~(1<<bitPos);
	}
	else
	{
		(*item) |= 1<<bitPos;
	}
}

void updateLight(char type, char val)
{
	switch(type)
	{
		case '0': /* zwykle swiatla */
			setBitVal(&lightFrame.message.frame.data0,3,val);
			break;

		case '1': /* dlugie */
			setBitVal(&lightFrame.message.frame.data0,5,val);
			break;

		case '2': /* kierunek lewy */
			setBitVal(&lightFrame.message.frame.data0,0,val);
			break;

		case '3': /* kierunek prawy */
			setBitVal(&lightFrame.message.frame.data0,1,val);
			break;

		case '4': /* przmg przod */
			setBitVal(&lightFrame.message.frame.data0,6,val);
			break;

		case '5': /* przmg tyl */
			setBitVal(&lightFrame.message.frame.data0,7,val);
			break;
	}
}

void updateWarning(char type, char val)
{
	switch(type)
	{
		case '0': /* aku */
			setBitVal(&carErrorFrame.message.frame.data0,7,val);
			break;

		case '1': /* drift */
			setBitVal(&carErrorFrame.message.frame.data0,3,val);
			break;

		case '2': /* espoff */
			setBitVal(&carErrorFrame.message.frame.data0,2,val);
			break;

		case '3': /* esperr */
			setBitVal(&carErrorFrame.message.frame.data0,1,val);
			break;

		case '4': /* abs */
			setBitVal(&carErrorFrame.message.frame.data0,0,val);
			break;

		case '5': /* oil */
			setBitVal(&carErrorFrame.message.frame.data1,2,val);
			break;

		case '6': /* check */
			setBitVal(&carErrorFrame.message.frame.data1,0,val);
			break;

		case '7': /* airbag */
			setBitVal(&airbagFrame.message.frame.data0,3,val);
			break;

		case '8': /* ham */
			setBitVal(&hoodBrakeFrame.message.frame.data0,4,val);
			break;

		case '9': /* cruise */
			setBitVal(&carErrorFrame.message.frame.data1,7,val);
			break;
	}
}


void btnHandle(void)
{
	if(buttonTime>0)
	{
		buttonTime--;

		if(buttonTime==0)
		{
			HAL_GPIO_WritePin(GPIOB, GPIO_PIN_14, GPIO_PIN_RESET);
		}
	}
}
