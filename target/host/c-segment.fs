\ $Id$

\ This file is part of Attila, a retargetable threaded interpreter
\ Copyright (c) 2007--2011, Simon Dobson <simon.dobson@computer.org>.
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

\ Segment manager using standard library functions This essentially
\ just exposes malloc() at Forth level, but is typically used to allocate
\ larger blocks of memory that are then managed further.

CHEADER:
#include <stdlib.h>
;CHEADER

\ Create a new segment, returning an address and size or 0 if it fails
C: CREATE-SEGMENT prim_create_segment ( n -- )
  CELL addr;

  addr = (CELL) malloc(n);
  if(addr == NULL) {
    PUSH_CELL(0);
  } else {
    PUSH_CELL(addr);
    PUSH_CELL(n);
  }
;C 
