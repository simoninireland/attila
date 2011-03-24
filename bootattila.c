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

// Attila main setup, bootstrapped version
//
// The start-up sequence at this level is minimal:
// 1. Allocate and initialise the memory block
// 2. Allocate space for a cold-start vector at the bottom of memory,
//    which will be filled-in when the dictionary is bootstrapped
// 3. Allocate the return and data stacks, initialising their pointers
// 4. Allocate the user variable area
// 5. Initialise the dictionary, which should also initialise the
//    cold-start vector
// 6. Start the inner interpreter, assumed to be a primitive call docolon,
//    executing the xt in the cold-start vector

#include "vm.h"


// ---------- Globals ----------

BYTEPTR top, bottom;
XTPTR ip;
CELLPTR data_stack_base, data_stack;
XTPTR return_stack_base, return_stack;
CELLPTR user;


// ---------- Main routine ----------

int
main( int argc, char **argv ) {
  // initialise memory and stacks
  bottom = init_memory(MEM_SIZE);
  top = bottom + CELL_SIZE;  // leave space for the cold-start vector
  return_stack_base = return_stack = top;   top += RETURN_STACK_SIZE;
  data_stack_base = data_stack = top;   top += DATA_STACK_SIZE;
  user = top;   top += USER_SIZE;
  init_io();

  // initialise dictionary
  init_dictionary();

  // start the outer interpreter
  ip = bottom;
  docolon();

  exit(0);
}
