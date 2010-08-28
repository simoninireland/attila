// $Id$

// Basic implementation for host gcc compiler

#include <stdlib.h>
#include <stdio.h>
#include <string.h>
#include <setjmp.h>

// ---------- Type mapping ----------

// Base types
typedef void VOID;
typedef long CELL;
typedef long long DOUBLE_CELL;
typedef VOID *XT;
typedef unsigned char BYTE;
typedef unsigned char CHARACTER;

// Derived pointer types
typedef CELL *CELLPTR;
typedef XT *XTPTR;
typedef BYTE *BYTEPTR;
typedef CHARACTER *CHARACTERPTR;
typedef VOID (*PRIMITIVE)( XT _xt );


// ---------- Interpreter support ----------

// Variables
XTPTR ip;      // interpreter instruction pointer
jmp_buf env;   // warm-start longjmp target

// Primitive calling macro
#define CALL( p ) p(_xt)

// Fatal error re-start
#define DIE( msg ) (printf("FATAL: %s\n", msg), longjmp(env, 0), (CELL) 0)


// ---------- Stacks ----------

// Variables
CELLPTR data_stack, data_stack_base;     // data stack
XTPTR return_stack, return_stack_base;   // return stack

// Data stack macros
#define PUSH_CELL( cell ) (*(data_stack++)) = ((CELL) cell)
#define POP_CELL() (CELL) ((--data_stack < data_stack_base) ? DIE("data stack underflow") : (*data_stack))
#define DATA_STACK_ITEM( n ) (CELLPTR) (data_stack - (n) - 1) 
#define DATA_STACK_DEPTH() (data_stack - data_stack_base)
#define DATA_STACK_RESET() data_stack = data_stack_base

// Return stack macros
#define PUSH_RETURN( xt ) (*(return_stack++)) = ((XT) xt)
#define POP_RETURN() (XT) ((--return_stack < return_stack_base) ? DIE("return stack underflow") : (*return_stack))
#define RETURN_STACK_DEPTH() (return_stack - return_stack_base)
#define RETURN_STACK_ITEM( n ) (XTPTR) (return_stack - (n) - 1)
#define PEEK_RETURN() (XT) *(RETURN_STACK_ITEM(0))
#define RETURN_STACK_RESET() return_stack = return_stack_base


// ---------- General helper routines ----------

// Convert an Attila-style counted string to a C-style zero-terminated string
CHARACTERPTR
create_unix_string( CELL addr, CELL namelen ) {
  static CHARACTER namebuf[256];

  strncpy(namebuf, (CHARACTERPTR) addr, namelen);   namebuf[namelen] = '\0';
  return namebuf;
}


// ---------- Start-up ----------

extern CELL image[];
extern CELLPTR user_variable( int );

int
main( int argc, char *argv[] ) {
  XT _xt = NULL;

  // set up the stacks
  data_stack_base = data_stack = image[USER__DATA_STACK_];
  return_stack_base = return_stack = image[USER__RETURN_STACK_];
  printf("%d -- %$d\n", data_stack, return_stack);

  // initialise the I/O streams
  *user_variable(USER_INPUTSOURCE) = (CELL) stdin;
  *user_variable(USER_OUTPUTSINK) = (CELL) stdout;
 
  // call the cold-start routine:
  ip = (XTPTR) user_variable(USER_COLDSTART);
  CALL(docolon);

  exit(0);
}
