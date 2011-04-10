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

// Basic interactive main() routine. This makes use of standard library
// functions like longjmps and signals to try to stay running in the
// presence of memory and other errors common in developing Forth code. 

#ifndef STDC_MAIN
#define STDC_MAIN

#include <stdlib.h>
#include <setjmp.h>
#include <signal.h>


// Warm-start vector
jmp_buf env; 

// Image and user variables
extern CELL image[];
extern CELLPTR user_variable( int );

// Forth level start word
extern VOID init( XT );


// Signal handler warm-starts the interpreter by jumping to the
// installed warm-srart longjmp with a handy message. Only the
// common signals are caught: others do their default action
void
signal_handler( int sig ) {
  switch(sig) {
  case SIGINT:
    DIE("Interrupt");

  case SIGBUS:
  case SIGSEGV:
    DIE("Memory violation");
  }
}


// Main routine
int
main( int argc, char *argv[] ) {
  XT _xt = NULL;
  CELL tmpstack[3];

  // install signal handlers
  signal(SIGINT, &signal_handler);
  signal(SIGBUS, &signal_handler);
  signal(SIGSEGV, &signal_handler);

  // set up a temporary stack, enough to call simple primitives
  // so we can d user variable calculations etc
  data_stack_base = data_stack = tmpstack;

  // initialise stacks to correct addresses
  data_stack_base = data_stack = *user_variable(USER__DATA_STACK_);
  return_stack_base = return_stack = *user_variable(USER__RETURN_STACK_);

  // call the interpreter cold-start routine
  CALL(init);

  exit(0);
}

#endif
