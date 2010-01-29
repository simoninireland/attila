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

// Macros etc needed by primgen for generating primitives

#ifndef PRIMGEN_H
#define PRIMGEN_H

// ---------- Data stack access ----------
// sd: These versions have no over- or under-flow checking
 
#define PUSH_CELL( cell ) (*(data_stack++)) = ((CELL) cell)
#define POP_CELL() (CELL) (*(--data_stack))
#define DATA_STACK_ITEM( n ) (CELLPTR) (data_stack - n - 1) 
#define DATA_STACK_DEPTH() ((data_stack - data_stack_base) / sizeof(CELL))
#define DATA_STACK_RESET() data_stack = data_stack_base

// ---------- Return stack access ----------
// sd: These versions have no over- or under-flow checking
 
#define PUSH_RETURN( xt ) (*(return_stack++)) = ((XT) xt)
#define POP_RETURN() (XT) (*(--return_stack))
#define PEEK_RETURN() (XT) (*return_stack)

// ---------- Primitive calling ----------

#define CALL( p ) p(_xt)

#endif
