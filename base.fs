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

\ Things that don't need to be strictly primitive but effectively are
\
\ We need this because Attila keeps its primitives and initial
\ dictionary literally as simple as possible: pretty much any words
\ that *can* be defined as source code, *are*. Of course where this
\ line is drawn is pretty arbitrary: you can define NIP as SWAP DROP,
\ but for efficiency it's probably better as a primitive. Still, the
\ principle is clear

\ ---------- Derived arithmetic operators ----------
    
\ Divide a by b as integers
: / \ ( a b -- a/b )
    /MOD NIP ;

\ Remainder of a divided by b
: MOD \ ( a b -- a%b )
    /MOD DROP ;

\ Multiply a by b and divide by c, using a double-precision intermediate value
: */ \ ( a b c -- (a*b)/c )
    */MOD NIP ;


\ ---------- Block sizes ----------

\ The number of bytes needed to represent n cells
: CELLS \ ( n -- bs )
    /CELL * ;

\ The number of bytes needed to represent n cells
: CHARS \ ( n -- bs )
    /CHAR * ;


\ ---------- Filling memory ----------

\ Fill an n-byte block of memory with a given byte value (usually 0). Safe
\ for zero and negative amounts
: CFILL ( addr n v -- )
    ROT 1- BEGIN
	DUP 0 >=
    WHILE
	    >R 2DUP R@ + C!
	    R> 1-
    REPEAT
    DROP 2DROP ;

	