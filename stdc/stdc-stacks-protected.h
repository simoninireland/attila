// $Id$

// This file is part of Attila, a retargetable threaded interpreter
// Copyright (c) 2007--2011, Simon Dobson <simon.dobson@computer.org>.
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

// Stacks without bounds checking, i.e. a sensible implementation....

#ifndef STDC_STACKS_PROTECTED
#define STDC_STACKS_PROTECTED

// Variables
CELLPTR data_stack, data_stack_base;     // data stack
XTPTR return_stack, return_stack_base;   // return stack

// Data stack macros
#define PUSH_CELL( cell ) ((data_stack >= (data_stack_base + DATA_STACK_SIZE)) ? DIE("data stack overflow") : ((*(data_stack++)) = ((CELL) cell)))
#define POP_CELL() (CELL) ((--data_stack < data_stack_base) ? DIE("data stack underflow") : (*data_stack))
#define DATA_STACK_ITEM( n ) (CELLPTR) (data_stack - (n) - 1) 
#define DATA_STACK_DEPTH() (data_stack - data_stack_base)
#define DATA_STACK_RESET() data_stack = data_stack_base

// Return stack macros
#define PUSH_RETURN( xt ) ((return_stack >= (return_stack_base + RETURN_STACK_SIZE)) ? DIE("return stack overflow") : ((*(return_stack++)) = ((XT) xt)))
#define POP_RETURN() (XT) ((--return_stack < return_stack_base) ? DIE("return stack underflow") : (*return_stack))
#define RETURN_STACK_DEPTH() (return_stack - return_stack_base)
#define RETURN_STACK_ITEM( n ) (XTPTR) (return_stack - (n) - 1)
#define PEEK_RETURN() (XT) *(RETURN_STACK_ITEM(0))
#define RETURN_STACK_RESET() return_stack = return_stack_base

#endif

