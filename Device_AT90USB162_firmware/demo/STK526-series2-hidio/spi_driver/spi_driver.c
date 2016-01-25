#include "spi_driver.h"

void SPI_Slave_Init(void)
{
   SPI_DDR  = (1<<SPI_MISO)|(0<<SPI_MOSI)|(0<<SPI_SCK)|(0<<SPI_SS);
   SPI_PORT = (0<<SPI_MISO)|(0<<SPI_MOSI)|(0<<SPI_SCK)|(0<<SPI_SS);
   
   SPCR = (1<<SPIE)|(1<<SPE);
           // SPIE, SPE, DORD, MSTR, CPOL, CPHA, SPR1, SPR0)
}

void SPI_Master_Init(void)
{
   SPI_DDR  |= (0<<SPI_MISO)|(1<<SPI_MOSI)|(1<<SPI_SCK)|(1<<SPI_SS);
   SPI_PORT |= (0<<SPI_MISO)|(0<<SPI_MOSI)|(0<<SPI_SCK)|(0<<SPI_SS);
                  //SPIE, SPE, DORD, MSTR, CPOL, CPHA, SPR1, SPR0)
   SPCR = (0<<SPIE)|(1<<SPE)|(1<<MSTR)|(0<<CPOL)|(0<<CPHA)|(0<<SPR1)|(0<<SPR0);
   SPSR = (1<<SPI2X);
}

U8 SPI_Send_Read_Byte(U8 data_send)
{   
   //SPI_PORT &= ~(1<<SPI_SS); //nRF24L01 виконує
   SPDR = data_send; // після запису данних в SPDR(SPI data register) вони відсилаються(регістер зсуває біти 8м раз, надсилаючи байт)
   while(!(SPSR & (1<<SPIF)));
   //SPI_PORT |= (1<<SPI_SS);  //nRF24L01 виконує
   return SPDR;
}

void SPI_Send_Array(/*U8 *data_send, U8 len*/)
{
   U8 len = 32;
   while(len--)
   {
      SPDR = Usb_read_byte();//*data_send++;// після запису данних в SPDR(SPI data register) вони відсилаються(регістер зсуває біти 8м раз, надсилаючи байт)
      while(!(SPSR & (1<<SPIF))); //nRF24L01 виконує
   }
   //Usb_ack_receive_out();
  /*
  // SPI_PORT &= ~(1<<SPI_SS);   //nRF24L01 виконує
   while(len--)
   {
      SPDR = *data_send++;// після запису данних в SPDR(SPI data register) вони відсилаються(регістер зсуває біти 8м раз, надсилаючи байт)
      while(!(SPSR & (1<<SPIF))); //nRF24L01 виконує
   }*/
  // SPI_PORT |= (1<<SPI_SS); 
}
/*
void SPI_Send_Array(U8 *data_send, U8 len)
{
  // SPI_PORT &= ~(1<<SPI_SS);   //nRF24L01 виконує
   while(len--)
   {
      SPDR = *data_send++;// після запису данних в SPDR(SPI data register) вони відсилаються(регістер зсуває біти 8м раз, надсилаючи байт)
      while(!(SPSR & (1<<SPIF))); //nRF24L01 виконує
   }
  // SPI_PORT |= (1<<SPI_SS); 
}*/

void SPI_Read_Array(/*U8 *data_read, U8 len*/)
{  

  U8 len = 32;
  Usb_select_endpoint(EP_HID_IN);
   while(len--)
   {
      SPDR = len;//*data_read;// після запису данних в SPDR(SPI data register) вони відсилаються(регістер зсуває біти 8м раз, надсилаючи байт)
      while(!(SPSR & (1<<SPIF)));
      //*data_read++ = SPDR;
      Usb_write_byte(SPDR);    
   }
  Usb_ack_in_ready();
     
   //SPI_PORT &= ~(1<<SPI_SS);  //nRF24L01 виконує
   /*while(len--)
   {
      SPDR = *data_read;// після запису данних в SPDR(SPI data register) вони відсилаються(регістер зсуває біти 8м раз, надсилаючи байт)
      while(!(SPSR & (1<<SPIF)));
      *data_read++ = SPDR; 
   }*/
  // SPI_PORT |= (1<<SPI_SS);   //nRF24L01 виконує
}

/*
void SPI_WriteArray(U8 *data_, U8 num)
{
   SPI_PORT &= ~(1<<SPI_SS); 
   while(num--)
   {
      SPDR = *data_++;
      while(!(SPSR & (1<<SPIF)));
   }
   SPI_PORT |= (1<<SPI_SS); 
}

void SPI_ReadArray(U8 *data_, U8 len)
{
   SPI_PORT &= ~(1<<SPI_SS); 
   while(len--)
   {
      SPDR = *data_;
      while(!(SPSR & (1<<SPIF)));
      *data_++ = SPDR; 
   }
   SPI_PORT |= (1<<SPI_SS); 
}*/
