#include "nRF24L01.h"

//activ is low
#define TX_POWER_UP nRF24L01_config_register(CONFIG,  (0<<MASK_RX_DR)|(1<<MASK_TX_DS)|(1<<MASK_MAX_RT)|(2<<EN_CRC)|(1<<CRCO)|(1<<PWR_UP)|(0<<PRIM_RX))
#define RX_POWER_UP nRF24L01_config_register(CONFIG,  (0<<MASK_RX_DR)|(1<<MASK_TX_DS)|(1<<MASK_MAX_RT)|(2<<EN_CRC)|(1<<CRCO)|(1<<PWR_UP)|(1<<PRIM_RX))
// Defines for setting the nRF24L01 registers for transmitting or receiving mode
//#define TX_POWER_UP nRF24L01_config_register(CONFIG, nRF24L01_CONFIG | ( (1<<PWR_UP) | (0<<PRIM_RX) ) )
//#define RX_POWER_UP nRF24L01_config_register(CONFIG, nRF24L01_CONFIG | ( (1<<PWR_UP) | (1<<PRIM_RX) ) )


// Flag which denotes transmitting mode
volatile U8 PTX;

void delay_ms(unsigned int delay)
{
    while (delay--)
        __delay_cycles(FOSC);
}

const U8 delay_one_usec = FOSC/1000;
void delay_us(unsigned int delay)
{
    while (delay--)
        __delay_cycles(delay_one_usec);
}

void nRF24L01_init() 
// Initializes pins ans interrupt to communicate with then RF24L01 module
// Should be called in the early initializing phase at startup.
{    
    // Define CSN and CE as Output and set them to default
    DDRB |= ((1<<CSN)|(1<<CE));
    nRF24L01_TX_mode;
    nRF24L01_CSN_hi;
    delay_ms(10);
    
    SPI_Master_Init();// Initialize spi module
    
    //__enable_interrupt();
    DDRC &= ~(1<<PC5);
    PCMSK1 = (1<<PCINT9);
    PCICR  = (1<<PCIE1);
    delay_ms(10);
}


void nRF24L01_config() 
// Sets the important registers in the MiRF module and powers the module
// in receiving mode
{
    // Set RF channel
    nRF24L01_config_register(RF_CH, nRF24L01_CH);

    //nRF24L01_config_register(RF_SETUP, (1<<RF_DR)|(0<<RF_PWR)|(0<<LNA_HCURR));
    
    // Set length of incoming payload 
    nRF24L01_config_register(RX_PW_P0, nRF24L01_PAYLOAD);
    
    // Set RADDR and TADDR
    nRF24L01_write_register(RX_ADDR_P0, TADDR, 5);
    //nRF24L01_write_register(RX_ADDR_P1, RADDR, 5);
    nRF24L01_write_register(TX_ADDR, TADDR, 5);
	
    // Enable RX_ADDR_P0 and RX_ADDR_P1 address matching since we also enable auto acknowledgement
    
    nRF24L01_config_register(EN_RXADDR, (1<<ERX_P0));//select pipe
    
    // Start receiver 
    PTX = 0;         // Start in receiving mode
    RX_POWER_UP;     // Power up in receiving mode
    nRF24L01_RX_mode;// Listening for pakets
}

#pragma vector = PCINT1_vect
__interrupt void PCINT1_vect_interrupt (void)
{  
  
    // If still in transmitting mode then finish transmission
    //if (PTX) 
    { 
        /*
        // Read nRF24L01 status 
        nRF24L01_CSN_lo;                                // Pull down chip select
        U8 status = SPI_Send_Read_Byte(NOP);               // Read status register
        nRF24L01_CSN_hi;     
	// Pull up chip select
	*/
        //nRF24L01_CSN_lo;                    // Pull down chip select
        //SPI_Send_Read_Byte( FLUSH_RX );         // Write cmd to flush tx fifo
        //nRF24L01_CSN_hi;                    // Pull up chip select

	//U8 dat[nRF24L01_PAYLOAD];
        //
	
	//if(Is_usb_write_enabled()) 
	   nRF24L01_read_data(/*dat*/); 
	//int len = 32;  
	  // while(len--)
          // Usb_write_byte( '0' + len );  
	
	/*Usb_select_endpoint(EP_HID_IN);
        if(Is_usb_write_enabled()) 
	{
	  for(int i = 0; i < nRF24L01_PAYLOAD; i++)
           Usb_write_byte( dat[i] );  // return;   // Not ready to send report
	}
        Usb_ack_in_ready();          
	*/
       // nRF24L01_TX_mode;                        // Deactivate transreceiver
        //RX_POWER_UP;                             // Power up in receiving mode
        //nRF24L01_RX_mode;                       // Listening for pakets
        //PTX = 0;                                // Set to receiving mode

        // Reset status register for further interaction
        //nRF24L01_config_register(STATUS, (0<<RX_DR)|(1<<TX_DS)|(1<<MAX_RT)); // Reset status register
	//nRF24L01_config_register(STATUS, (0<<RX_DR)|(1<<TX_DS)|(1<<MAX_RT)|(000<<RX_P_NO)|(0<<TX_FULL)); // Reset status register
    }
}

void nRF24L01_read_data(/*U8 *read_data*/) 
// Reads nRF24L01_PAYLOAD bytes into data array
{  
    nRF24L01_CSN_lo;                             // Pull down chip select
    SPI_Send_Read_Byte( R_RX_PAYLOAD );          // Send cmd to read rx payload
    SPI_Read_Array(/*read_data, nRF24L01_PAYLOAD*/); // Read payload
    nRF24L01_CSN_hi;                             // Pull up chip select
    nRF24L01_config_register(STATUS, (1<<RX_DR));// Reset status register
}

void nRF24L01_send_data(/*U8 *value, U8 len*/)
// Sends a data package to the default address. Be sure to send the correct
// amount of bytes as configured as payload on the receiver.
{
   // while (PTX);                  // Wait until last paket is send

    nRF24L01_TX_mode;   // Set to transmitter mode
    //PTX = 1;                        
    TX_POWER_UP;        // Power up
    
    //nRF24L01_CSN_lo;                    // Pull down chip select
    //SPI_Send_Read_Byte( FLUSH_TX );         // Write cmd to flush tx fifo
    //nRF24L01_CSN_hi;                    // Pull up chip select
    
    nRF24L01_CSN_lo;                    // Pull down chip select
    SPI_Send_Read_Byte( W_TX_PAYLOAD ); // Write cmd to write payload
    SPI_Send_Array(/*value, len*/);         // Write payload
    nRF24L01_CSN_hi;                    // Pull up chip select
    
    nRF24L01_RX_mode; // Start transmission
    delay_us(270);     //for Start  transmission
    RX_POWER_UP; 
}

void nRF24L01_set_RADDR(U8 *adr) 
// Sets the receiving address
{
    nRF24L01_TX_mode;
    U8 adr_len = 5;
    nRF24L01_write_register(RX_ADDR_P0, adr, adr_len);
    nRF24L01_RX_mode;
}

void nRF24L01_set_TADDR(U8 *adr)
// Sets the transmitting address
{
    U8 adr_len = 5;
    nRF24L01_write_register(TX_ADDR, adr, adr_len);
}


bool nRF24L01_data_ready() 
// Checks if data is available for reading
{
    if (PTX) 
      return false;
    
    // Read nRF24L01 status 
    nRF24L01_CSN_lo;                     // Pull down chip select
    U8 status = SPI_Send_Read_Byte(NOP); // Read status register
    nRF24L01_CSN_hi;                     // Pull up chip select
    return status & (1<<RX_DR);
}

void nRF24L01_config_register(U8 reg, U8 value)
// Clocks only one byte into the given MiRF register
{
    nRF24L01_CSN_lo;
    SPI_Send_Read_Byte(W_REGISTER | (REGISTER_MASK & reg));
    SPI_Send_Read_Byte(value);
    nRF24L01_CSN_hi;
}

void nRF24L01_read_register(U8 reg, U8 *value, U8 len)
// Reads an array of bytes from the given start position in the nRF24L01 registers.
{
    nRF24L01_CSN_lo;
    SPI_Send_Read_Byte(R_REGISTER | (REGISTER_MASK & reg));
    SPI_Read_Array(value, len);
    nRF24L01_CSN_hi;
}

void nRF24L01_write_register(U8 reg, U8 *value, U8 len) 
// Writes an array of bytes into inte the nRF24L01 registers.
{
    nRF24L01_CSN_lo;
    SPI_Send_Read_Byte(W_REGISTER | (REGISTER_MASK & reg));
    //SPI_Send_Array(value, len);
    U8 *data_send = value; 
    while(len--)
    {
      SPDR = *data_send++;// після запису данних в SPDR(SPI data register) вони відсилаються(регістер зсуває біти 8м раз, надсилаючи байт)
      while(!(SPSR & (1<<SPIF))); //nRF24L01 виконує
    }
    nRF24L01_CSN_hi;
}
