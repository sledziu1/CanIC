/*
 * receiver.c
 *
 *  Created on: 08.12.2018
 */

#include "stdint.h"
#include "sender.h"
#include "stdlib.h"
#include "stm32f1xx_hal.h"

void processMessage(const char * const buf, const uint8_t size);

#define BUF_SIZE 60

#define SEQ_START '*'
#define SEQ_STOP '#'

static char buffer[BUF_SIZE];
static uint8_t bufPos=0;
static uint8_t inSequence=0;




void addChar(char ch)
{
	if(ch==SEQ_START) /* SEQ_START resets seq all the time */
	{
		inSequence=1;
		bufPos=0;
	}
	else if(inSequence==1) /* in sequence - process char*/
	{
		if(ch==SEQ_STOP)
		{
			inSequence=0;
			/* call func*/
			buffer[bufPos]=0; /* for atoi */
			processMessage(buffer,bufPos);
		}
		else if(bufPos<BUF_SIZE-1) /* keep one space for 0delimetr for atoi func */
		{
			buffer[bufPos]=ch;
			bufPos++;
		}
		else
		{
			/* no more space in buffer, wrong message, reset */
			inSequence=0;
			bufPos=0;
		}
	}
	else
	{
		/* not in sequence, skip char */
	}
}

void processMessage(const char * const buf, const uint8_t size)
{
	if(buf[0]=='s' && size<=5) /* speed frame */
	{
		uint32_t tmp = atoi(buf+1);
		updateSpeed(tmp);
	}
	else if(buf[0]=='r' && size<=6) /* rmp frame */
	{
		uint32_t tmp = atoi(buf+1);
		updateRpm(tmp);
	}
	else if(buf[0]=='g' && size<=6) /* gear frame */
	{
		uint32_t tmp = atoi(buf+1);
		updateGear(tmp);
	}
	else if(buf[0]=='t' && size<=6) /* outside temp frame */
	{
		int16_t tmp = atoi(buf+1);
		updateOutsideTemp(tmp);
	}
	else if(buf[0]=='f' && size<=6) /* fuel frame */
	{
		int16_t tmp = atoi(buf+1);
		updateFuel(tmp);
	}
	else if(buf[0]=='h' && size<=6) /* handbrake */
	{
		if(buf[1]=='1')
		{
			HAL_GPIO_WritePin(GPIOB, GPIO_PIN_13, GPIO_PIN_SET);
		}
		else
		{
			HAL_GPIO_WritePin(GPIOB, GPIO_PIN_13, GPIO_PIN_RESET);
		}
	}
	else if(buf[0]=='e' && size<=6) /* enable */
	{
		if(buf[1]=='0')
		{
			HAL_GPIO_WritePin(GPIOB, GPIO_PIN_12, GPIO_PIN_SET);
		}
		else
		{
			HAL_GPIO_WritePin(GPIOB, GPIO_PIN_12, GPIO_PIN_RESET);
		}
	}
	else if(buf[0]=='b' && size<=6) /* button */
	{
		if(buf[1]=='0')
		{
			btnPress(0);
		}
		else
		{
			btnPress(1);
		}
	}
	else if(buf[0]=='l' && size<=6) /* lights */
	{
		updateLight(buf[1],buf[2]);
	}
	else if(buf[0]=='w' && size<=6) /* warning */
	{
		updateWarning(buf[1],buf[2]);
	}


}


