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

\ Null-terminated strings
\
\ Routines to convert to and from the usual counted string format
\ and the C style of null-terminated string (zstrings). Useful for
\ accessing  low-level routines, but also for storing strings longer
\ than the single-byte count of "normal" strings

\ The NULL character
0 CONSTANT NULLCHAR

\ Find the end address of a zstring -- the address of the terminating
\ null
: Z> \ ( zaddr -- zaddr1 )
    BEGIN
	DUP C@ NULLCHAR <>
    WHILE
	    1+
    REPEAT ;

\ Convert a zstring to a normal address-plus-count
\ The count may be more than a single byte
: ZCOUNT \ ( zaddr -- addr n )
    DUP Z> OVER - ;

\ Move a zstring at zaddr1 to zaddr2, assuming there's enough memory
: ZMOVE \ ( zaddr1 zaddr2 -- )
    OVER ZCOUNT 1+ NIP CMOVE ;

\ Concatenate zstring2 to the end of zstring1, assuming there's
\ enough memory
: Z+ \ ( zaddr1 zaddr2 -- )
    2DUP >R Z> R> ZCOUNT >R SWAP R> CMOVE
    >R ZCOUNT
    R> ZCOUNT NIP + + 0 SWAP C! ; 
    
\ Concatenate a string to the end of a zstring, assuming there's
\ enough memory
: Z+S \ ( zaddr addr n -- )
    >R >R ZCOUNT R> R>        \ zaddr zn addr n
    DUP -ROT                  \ zaddr zn n addr n
    4 PICK Z> SWAP CMOVE      \ zaddr zn n
    + + NULLCHAR SWAP C! ;

\ Convert a string to a zstring at zaddr, assuming there's enough memory
: S>Z \ ( addr n zaddr -- )
    DUP >R
    SWAP DUP >R CMOVE
    NULLCHAR R> R> + C! ;

\ Compile a zstring into data memory
: Z, \ ( zaddr -- )
    ALIGNED HERE SWAP
    ZCOUNT 1+ ALLOT
    SWAP ZMOVE ;

\ Display a zstring
: ZTYPE \ ( zaddr -- )
    ZCOUNT TYPE ;
