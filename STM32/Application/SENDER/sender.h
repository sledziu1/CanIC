/*
 * sender.h
 *
 *  Created on: 06.12.2018
 */

#ifndef APPLICATION_USER_SENDER_H_
#define APPLICATION_USER_SENDER_H_

extern void updateSpeed(uint16_t speed);

extern void updateRpm(uint16_t rpm);

extern void updateGearUp(uint32_t v);

extern void updateGear(uint32_t v);

extern void updateOutsideTemp(int16_t temp);

extern void updateFuel(int16_t fuel);

extern void updateLight(char type, char val);

extern void updateWarning(char type, char val);

extern void run(void);

extern void btnPress(uint8_t type);

extern void LoadCanFramesDefaultData(void);

extern void handleMsTick(void);


#endif /* APPLICATION_USER_SENDER_H_ */
