\ $Id$

\ This file is part of Attila, a retargetable threaded interpreter
\ Copyright (c) 2007--2010, Simon Dobson <simon.dobson@computer.org>.
\ All rights reserved.
\
\ Attila is free software; you can redistribute it and/or
\ modify it under the terms of the GNU General Public License
\ as published by the Free Software Foundation; either version 2
\ of the License, or (at your option) any later version.
\
\ Attila is distributed in the hope that it will be useful,
\ but WITHOUT ANY WARRANTY; without even the implied warranty of
\ MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
\ GNU General Public License for more details.
\
\ You should have received a copy of the GNU General Public License
\ along with this program; if not, write to the Free Software
\ Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA 02111, USA.

\ Continuation capturing primitives
\
\ These two primitives grab and restore the data and return stacks, using
\ the portable stack primitives.
\
\ There are a few things to note.
\   1. These are the first steps of the process, which usually requires
\      other words as defined in continuations.fs
\   2. These primitives run first, and so set the scene for the rest
\      of the process. (CAPTURE-STACKS) captures that stacks before the
\      other words on the capture hook run, so any stack effect they have
\      is not captured
\   3. We need to be careful about restoring the return stack so that we
\      continue into the right place

\ Capture the data and return stacks into a continuation. The xt is
\ left unchanged and isn't captured on the data stack; the initial
\ continuation is also returned
C: (CAPTURE-STACKS) cc ( xt cont -- xt cont acont )
  CELLPTR cp;
  CELLPTR ptr;
  int size, i;

  cp = (CELLPTR) cont;

  // capture data stack
  size = DATA_STACK_DEPTH();
  *(cp++) = (CELL) size;
  ptr = DATA_STACK_ITEM(0);
  for(i = size - 1; i >= 0; i--)
    *(cp++) = *DATA_STACK_ITEM(i);

  // capture return stack
  size = RETURN_STACK_DEPTH();
  *(cp++) = (CELL) size;
  ptr = RETURN_STACK_ITEM(0);
  for(i = size - 1; i >= 0; i--)
    *(cp++) = *RETURN_STACK_ITEM(i);

  acont = (CELL) cp;
;C


\ Restore the data and return stacks from a continuation
C: (RESTORE-STACKS) rc ( cont -- acont )
  CELLPTR cp;
  CELLPTR ptr;
  int size, i;

  cp = (CELLPTR) cont;

  // restore data stack
  size = (int) *(cp++);
  DATA_STACK_RESET();
  for(i = 0; i < size; i++)
    PUSH_CELL(*(cp++));

  // restore return stack
  size = (int) *(cp++);
  RETURN_STACK_RESET();
  for(i = 0; i < size; i++)
    PUSH_RETURN(*(cp++));

  acont = (CELL) cp;
;C
