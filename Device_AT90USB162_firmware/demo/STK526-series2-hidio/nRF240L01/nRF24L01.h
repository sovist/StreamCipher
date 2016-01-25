#ifndef _nRF24L01_H_
#define _nRF24L01_H_

#include "config.h"
#include "nRF24L01.inc"
#include "spi_driver/spi_driver.h"

/*
#define SPI_PORT   PORTB
#define SPI_DDR    DDRB

#define SPI_SS   0
#define SPI_SCK  1
#define SPI_MOSI 2
#define SPI_MISO 3 
*/
// nRF24L01 settings
#define nRF24L01_CH         2
#define nRF24L01_PAYLOAD    32
#define RADDR		(U8 *)"clnt1"
#define TADDR		(U8 *)"serv1"
//#define nRF24L01_CONFIG     ( (1<<MASK_RX_DR) | (1<<EN_CRC) | ~(1<<CRCO) )

// Pin definitions for chip select and chip enabled of the nRF24L01 module
#define CE   5 // 
#define CSN  0 // chip select 

// Definitions for selecting and enabling nRF24L01 module
#define nRF24L01_CSN_hi     PORTB |=  (1<<CSN);
#define nRF24L01_CSN_lo     PORTB &= ~(1<<CSN);
#define nRF24L01_RX_mode      PORTB |=  (1<<CE);
#define nRF24L01_TX_mode      PORTB &= ~(1<<CE);

// Public standart functions
void nRF24L01_init();
void nRF24L01_config();

void nRF24L01_set_RADDR(U8*);
void nRF24L01_set_TADDR(U8*);

bool nRF24L01_data_ready();

void nRF24L01_send_data(/*U8*, U8*/);
void nRF24L01_read_data(/*U8**/);

void nRF24L01_config_register(U8, U8);
 
void nRF24L01_read_register  (U8, U8*, U8);
void nRF24L01_write_register (U8, U8*, U8);
 
void delay_ms(unsigned int delay);
void delay_us(unsigned int delay);
#endif /* _nRF24L01_H_ */