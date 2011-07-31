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

\ Header navigation and construction for an indirect-threaded interpreter


\ ---------- Header navigation ----------

\ Convert an txt to the address of its code field (no-op in this model)
: >CFA ;

\ CFAs, stored as host pointers
: CFA@ \ ( txt -- cf )
    >CFA (CFA@) ;
: CFA! \ ( cf txt -- )
    >CFA (CFA!) ;

\ Convert a txt to a link pointer containing the xt of the next word in the
\ definition chain
: >LFA \ ( txt -- lfa )
    1 CELLS - ;

\ Convert a txt to its status field
: >STATUS \ ( txt -- addr )
    2 CELLS - 1+ ;

\ Convert an xt to a name string. addr will be CALIGNED
: >NAME \ ( txt -- addr namelen )
    >LFA 1 CELLS - DUP C@
    DUP >R
    - 1-
    1 CELLS - CALIGN
    R> ;

\ Convert a txt to indirect body (DOES> behaviour) if present
: >IBA \ ( txt -- iba )
    1 CELLS + ;

\ Access IBA
: IBA@ ( txt -- addr ) >IBA @ ;
: IBA! ( addr txt -- ) >IBA ! ;


\ ---------- Status and body management ----------

\ Mask-in the given mask to the status of a word
: SET-STATUS \ ( f txt -- )
    >STATUS DUP C@
    ROT OR
    SWAP C! ;

\ Get the status of the given word
: GET-STATUS \ ( txt -- s )
    >STATUS C@ ;

\ Convert txt to body address, accounting for iba if present
: >BODY \ ( xt -- addr )
    DUP >IBA
    SWAP REDIRECTABLE? IF
	1 CELLS +
    THEN ;


\ ---------- Header construction ----------

\ Cross-compile a header for the name word with the given code field
: (CROSS-HEADER,) \ ( addr n cf -- txt )
    CALIGNED                       \ start word on code cell boundary (if needed)
    >R                             \ stash the code field
    DUP >R                         \ compile the name
    ?DUP 0> IF
	0 DO
	    DUP [FORTH] C@ CCOMPILE,
	    1 [FORTH] CHARS +
	LOOP
    THEN
    DROP
    CALIGNED                       \ name and status on next cell boundary (if needed)
    R> CCOMPILE,                   \ compile the name length
    0 CCOMPILE,                    \ status byte
    CALIGNED                       \ link pointer on next cell boundary (if needed)
    LASTXT DUP 0= IF               \ compile the link pointer
	COMPILE,                   \ first word, LFA is 0
    ELSE
	ACOMPILE,                  \ other words, point to LASTXT 
    THEN 
    TOP                            \ the txt
    R> CFACOMPILE,                 \ the code pointer
    DUP LAST XT! ;                 \ update LAST

' (CROSS-HEADER,) IS (HEADER,)

