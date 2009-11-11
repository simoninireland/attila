// $Id: stack.c,v 1.4 2007/05/17 15:33:46 sd Exp $

// This file is part of Attila, a multi-targeted threaded interpreter
// Copyright (c) 2007, UCD Dublin. All rights reserved.
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

// Stack management

#include <stdlib.h>
#include "attila.h"

#define STACK_BLOWUP(st, top, n) if(((top) + (n) - ((st) + sizeof(size_t))) > (* ((size_t *) (st)))) { CRASH("Stack overflow"); }
#define STACK_BLOWDOWN(st, top, n) if((top) - (n) < ((st) + sizeof(size_t))) { CRASH("Stack underflow"); }


// Initialise a stack. We store the maximum size in the first cell for
// later sanity checking.
void
stack_init( byte **st, byte **top, size_t n ) {
  *st = (byte *) allocate_word_data_memory(n + sizeof(size_t));
  **((size_t **) st) = n;
  *top = (*st) + sizeof(size_t);
}


// Push an element onto a stack
void
stack_push( byte *st, byte **top, void *elem, size_t n ) {
  STACK_BLOWUP(st, *top, n);
  bcopy(elem, *top, n);
  *top += n;
}


// Pop an element off a stack
void
stack_pop( byte *st, byte **top, void *elem, size_t n ) {
  STACK_BLOWDOWN(st, *top, n);
  *top -= n;
  bcopy(*top, elem, n);
}


// Peek at the element on the top of a stack
void
stack_peek( byte *st, byte **top, void *elem, size_t n ) {
  STACK_BLOWDOWN(st, *top, -n);
  void *ptr = (*top) - n;
  bcopy(ptr, elem, n);
}


// Return a pointer to the topmost element
void
stack_topptr( byte *st, byte **top, void **elem, size_t n ) {
  STACK_BLOWDOWN(st, *top, -n);
  *elem = (*top) - n;
}


// Pick the i'th element off the stack, counting down from 0
void
stack_pick( byte *st, byte **top, int i, void *elem, size_t n ) {
  STACK_BLOWDOWN(st, *top, -(n * (i + 1)));
  void *ptr = (*top) - n * (i + 1);
  bcopy(ptr, elem, n);
}


// Roll the stack by i items, with the top item going to the i'th counting
// from 0
void
stack_roll( byte *st, byte **top, int i, size_t n ) {
  // only does anything for one or more
  if(i < 1) return;
  if(i >= stack_depth(st, top, n)) { CRASH("Stack too shallow"); }

  byte *ith = (*top) - n * (i + 1);
  byte *next = ith + n;
  byte *t;   stack_topptr(st, top, &t, n);
  byte buf[n];
  bcopy(t, buf, n);
  bcopy(ith, next, n * (i + 1));
  bcopy(buf, ith, n);
}


// Roll the stack by i items, with the i'th item coming to the top counting
// from 0
void
stack_backroll( byte *st, byte **top, int i, size_t n ) {
  // only does anything for one or more
  if(i < 1) return;
  if(i >= stack_depth(st, top, n)) { CRASH("Stack too shallow"); }

  byte *ith = (*top) - n * (i + 1);
  byte *next = ith + n;
  byte *t;   stack_topptr(st, top, &t, n);
  byte buf[n];
  bcopy(ith, buf, n);
  bcopy(next, ith, n * (i + 1));
  bcopy(buf, t, n);
}
 

// Return the depth of the stack in n-sized items
int
stack_depth( byte *st, byte **top, size_t n ) {
  return (*top - (st + sizeof(size_t))) / n;
}


// Re-set the stack
void
stack_reset( byte *st, byte **top ) {
  *top = st + sizeof(size_t);
}
