/*This file is prepared for Doxygen automatic documentation generation.*/
//! \file *********************************************************************
//!
//! \brief This file manages the generic HID IN/OUT task.
//!
//! - Compiler:           IAR EWAVR and GNU GCC for AVR
//! - Supported devices:  AT90USB162, AT90USB82
//!
//! \author               Atmel Corporation: http://www.atmel.com \n
//!                       Support and FAQ: http://support.atmel.no/
//!
//! ***************************************************************************

/* Copyright (c) 2009 Atmel Corporation. All rights reserved.
 *
 * Redistribution and use in source and binary forms, with or without
 * modification, are permitted provided that the following conditions are met:
 *
 * 1. Redistributions of source code must retain the above copyright notice,
 * this list of conditions and the following disclaimer.
 *
 * 2. Redistributions in binary form must reproduce the above copyright notice,
 * this list of conditions and the following disclaimer in the documentation
 * and/or other materials provided with the distribution.
 *
 * 3. The name of Atmel may not be used to endorse or promote products derived
 * from this software without specific prior written permission.
 *
 * 4. This software may only be redistributed and used in connection with an Atmel
 * AVR product.
 *
 * THIS SOFTWARE IS PROVIDED BY ATMEL "AS IS" AND ANY EXPRESS OR IMPLIED
 * WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF
 * MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NON-INFRINGEMENT ARE EXPRESSLY AND
 * SPECIFICALLY DISCLAIMED. IN NO EVENT SHALL ATMEL BE LIABLE FOR ANY DIRECT,
 * INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
 * (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
 * LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
 * ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF
 * THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 */

//_____  I N C L U D E S ___________________________________________________

#include "config.h"
#include "conf_usb.h"
#include "hid_task.h"
#include "lib_mcu/usb/usb_drv.h"
#include "usb_descriptors.h"
#include "modules/usb/device_chap9/usb_standard_request.h"
#include "usb_specific_request.h"
#include "lib_mcu/util/start_boot.h"

#include "spi_driver/spi_driver.h"
#include "nRF240L01/nRF24L01.h"

//_____ M A C R O S ________________________________________________________


//_____ D E F I N I T I O N S ______________________________________________


//_____ D E C L A R A T I O N S ____________________________________________

volatile          U8    cpt_sof=0;
extern            U8    jump_bootloader;
                  U8    g_last_joy       =   0;

void hid_report_out  (void);
void hid_report_in   (void);
void SPI_SlaveInit(void);

//! @brief This function initializes the target board ressources.
//!
void hid_task_init(void)
{
    nRF24L01_init();
    nRF24L01_config();

   //SPI_Slave_Init();
   //Leds_init();
   //Leds_set_val(0);
   
   //Joy_init();
   //Leds_set_val(0);
}

//! @brief Entry point of the HID generic communication task
//! This function manages IN/OUT repport management.
//!
void hid_task(void)
{
   if(!Is_device_enumerated())          // Check USB HID is enumerated
      return;  
   
   hid_report_out();
   hid_report_in();
}


//! @brief Get data report from Host
//!
/*
#pragma vector = SPI_STC_vect
__interrupt void SPI_STC_interrupt (void)
{ 
   Usb_select_endpoint(EP_HID_IN);
   if(!Is_usb_write_enabled())
      return;   // Not ready to send report  
   
   Usb_write_byte( SPDR );
   Usb_ack_in_ready();
}*/

void hid_report_out(void)
{
  
   Usb_select_endpoint(EP_HID_OUT);
   if(Is_usb_receive_out())
   {       
    //for(int i = 0; i < 32; i++)
      nRF24L01_send_data(/*sen, 32*/);
      Usb_ack_receive_out();
   }

   //** Check if we received DFU mode command from host
   if(jump_bootloader)
   {
      //Leds_off();
      Usb_detach();                             // Detach actual generic HID application
      for( U32 volatile tempo=0; tempo < 7000; tempo++);      // Wait some time before
      start_boot();                             // Jumping to booltoader
   }
}


//! @brief Send data report to Host
//!

void hid_report_in(void)
{
  /*
   Usb_select_endpoint(EP_HID_IN);
   if(!Is_usb_write_enabled())
      return;   // Not ready to send report
   
   Usb_ack_in_ready(); 
   */
}

//! @brief  This function increments the cpt_sof counter each times
//! the USB Start Of Frame interrupt subroutine is executed (1ms)
//! Usefull to manage time delays
//!
void sof_action()
{ cpt_sof++; }
