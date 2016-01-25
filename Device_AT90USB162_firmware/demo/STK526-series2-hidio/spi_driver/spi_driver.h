
#ifndef SPI_H
#define SPI_H

#include "config.h"
#include "conf_usb.h"
#include "lib_mcu/usb/usb_drv.h"

#define SPI_PORT   PORTB
#define SPI_DDR    DDRB

#define SPI_SS   0 //chip select
#define SPI_SCK  1 //clock
#define SPI_MOSI 2 //Master Out Slave In
#define SPI_MISO 3 //Master In Slave Out

void SPI_Master_Init(void); 
void SPI_Slave_Init(void);

  U8 SPI_Send_Read_Byte(U8 data_send);
void SPI_Send_Array(/*U8 *data_send, U8 len*/);  
void SPI_Read_Array(/*U8 *data_read, U8 len*/);

#endif //SPI_H


