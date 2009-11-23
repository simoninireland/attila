// $Id$

// This file is part of Attila, a retargetable threaded interpreter
// Copyright (c) 2007--2009, Simon Dobson <simon.dobson@computer.org>.
// All rights reserved.
//
// Attila is free software; you can redistribute it and/or
// modify it under the terms of the GNU General Public License
// as published by the Free Software Foundation; either version 2
// of the License, or (at your option) any later version.
//
// Attila is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA 02111, USA.

// The core virtual machine of Attila.

#ifndef VM_H
#define VM_H

// ---------- Thing that should come from somewhere else... ----------

// Base types
typedef void VOID;
typedef unsigned long CELL;
typedef unsigned long long DOUBLE_CELL;
typedef VOID *XT;
typedef unsigned char BYTE;
typedef unsigned char CHARACTER;

// Sizes in bytes
#define CELL_SIZE sizeof(CELL)
#define MEM_SIZE (48 * 2024)
#define DATA_STACK_SIZE (100 * CELL_SIZE)
#define RETURN_STACK_SIZE (100 * CELL_SIZE)
#define USER_SIZE (20 * CELL_SIZE)
#define TIB_SIZE 1024


// ---------- Memory management ----------

// Truth values
#define FALSE 0
#define TRUE (!FALSE)

// Derived pointer types
typedef CELL *CELLPTR;                 // cells
typedef XT *XTPTR;                     // execution tokens
typedef BYTE *BYTEPTR;                 // bytes 
typedef CHARACTER *CHARACTERPTR;       // characters
typedef VOID (*PRIMITIVE)( XT _xt );   // primitive code 

// Memory initialisation
extern BYTEPTR top, bottom;
extern BYTEPTR init_memory( CELL bs );

// Dictionary initialisation
extern VOID init_dictionary(); 


// ---------- Engine ----------

// The instruction pointer
extern XTPTR ip;

// The stack pointers
extern CELLPTR data_stack_base, data_stack;
extern XTPTR return_stack_base, return_stack;

// The user variable area
extern CELLPTR user;

// The primitive to execute a word on the data stack
extern VOID execute();

#endif
