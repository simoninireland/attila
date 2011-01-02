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

\ C bridging primitives for the flat memory model, C version.
\
\ This file exposes some of the flat memory model routines from
\ fat-memory-model.fs as C primitives, so they can be called from
\ the inner interpreter.

\ Align code address on cell boundaries
\ : CALIGN \ ( addr -- aaddr )
\     DUP /CELL MOD ?DUP 0<> IF /CELL SWAP - + THEN ;
C: (CALIGN) calign ( addr -- aaddr )
  aaddr = addr;
  if((aaddr % sizeof(CELL)) > 0)
    aaddr = aaddr + (sizeof(CELL) - (aaddr % sizeof(CELL)));
;C

\ Convert an xt to a link pointer containing the xt of the next word in the
\ definition chain
\ : >LFA \ ( xt -- lfa )
\     1 CELLS - ;
C: (>LFA) xt_to_lfa ( xt -- lfa )
  lfa = (CELL) ((BYTEPTR) xt - sizeof(CELL));
;C

\ Convert xt to its status field. The namelen and status are adjacent and
\ CALIGNED
\ : >STATUS \ ( xt -- addr )
\     2 CELLS - 1+ ;
C: (>STATUS) xt_to_status ( xt -- addr )
  addr = (CELL) ((BYTEPTR) xt - 2 * sizeof(CELL) + 1);
;C

\ Convert an xt to a name string. addr will be CALIGNED
\ : >NAME \ ( xt -- addr namelen )
\     >LFA 1 CELLS - DUP C@
\     DUP >R
\     -
\     DUP /CELL MOD 0<> IF
\         /CELL -
\     THEN CALIGN                \ ensure we're aligned on the previous cell boundary
\     R> ;
C: (>NAME) xt_to_name ( -- addr n )
  BYTEPTR a;

  CALL(xt_to_lfa);
  a = (BYTEPTR) POP_CELL();
  a -= sizeof(CELL);
  n = (CELL) *a;
  a -= n;
  if((n % sizeof(CELL)) > 0)
    a -= sizeof(CELL);
  PUSH_CELL(a);
  CALL(calign);
  addr = POP_CELL();
;C

\ Convert xt to indirect body (DOES> behaviour) if present
\ (We don't check whether there actually *is* an IBA)
\ : >IBA \ ( xt -- iba )
\     1 CELLS + ;
C: >IBA xt_to_iba ( xt -- iba )
    iba = (CELL) ((BYTEPTR) xt + sizeof(CELL));
;C

\ Convert xt to body address, accounting for iba if present. This
\ has to be coded as a primitive called xt_to_body, which is used
\ by the inner interpreter, and it *has* to match the other
\ definitions (>STATUS, REDIRECTABLE-MASK, IMMEDIATE?)  even though
\ it doesn't use them directly 
\ : >BODY \ ( xt -- addr )
\     DUP >IBA
\     SWAP REDIRECTABLE? IF
\ 	1 CELLS +
\     THEN ;
C: >BODY xt_to_body ( xt -- addr )
  BYTEPTR statusptr;
  BYTE status;

  statusptr = ((BYTEPTR) xt) - 2 * sizeof(CELL) + 1;
  status = *statusptr;
  addr = xt + sizeof(CELL);
  if(status & REDIRECTABLE_MASK)
    addr += sizeof(CELL);
;C
