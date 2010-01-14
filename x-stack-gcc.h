// $Id$

// Data and return stack macros
//
// Fast versions without bounds checking

#ifndef X_STACK_GCC_H
#define X_STACK_GCC_H

// ---------- Data stack access ----------
 
#define PUSH_CELL( cell ) (*(data_stack++)) = ((CELL) cell)
#define POP_CELL() (CELL) ((--data_stack < data_stack_base) ? DIE("data stack underflow") : (*data_stack))
#define DATA_STACK_ITEM( n ) (CELLPTR) (data_stack - (n) - 1) 
#define DATA_STACK_DEPTH() (data_stack - data_stack_base)
#define DATA_STACK_RESET() data_stack = data_stack_base


// ---------- Return stack access ----------
 
#define PUSH_RETURN( xt ) (*(return_stack++)) = ((XT) xt)
#define POP_RETURN() (CELL) ((--return_stack < return_stack_base) ? DIE("return stack underflow") : (*return_stack))
#define PEEK_RETURN() (XT) ((return_stack == return_stack_base) ? DIE("peeking empty return stack") : (*(return_stack - 1)))
#define RETURN_STACK_ITEM( n ) (XTPTR) (return_stack - n - 1) 
#define RETURN_STACK_RESET() return_stack = return_stack_base

#endif
