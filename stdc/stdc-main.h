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

// Basic interactive main() routine

#ifndef STDC_MAIN
#define STDC_MAIN

#include <stdlib.h>
#include <setjmp.h>


// Warm-start vector
jmp_buf env; 

// Image and user variables
extern CELL image[];
extern CELLPTR user_variable( int );

// Forth level start word
extern VOID init( XT );

int
main( int argc, char *argv[] ) {
  XT _xt = NULL;
  CELL tmpstack[3];

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
