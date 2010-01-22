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
#define CALL( p ) p(_xt);

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
#define PEEK_RETURN() (XT) ((return_stack == return_stack_base) ? DIE("peeking empty return stack") : (*(return_stack - 1)))
#define RETURN_STACK_ITEM( n ) (XTPTR) (return_stack - n - 1) 
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

int
main( int argc, char *argv[] ) {
  XT _xt = NULL;

  // hack the stacks, for now
  data_stack_base = data_stack = &image[20];
  return_stack_base = return_stack = &image[50];

  // also hack the I/O streams
  image[6] = (CELL) stdin;
  image[7] = (CELL) stdout;
 
  // call the cold-start routine:
  ip = (XTPTR) &image[0];         // cold-start vector is at the first cell in the image
  CALL(docolon);

  exit(0);
}
