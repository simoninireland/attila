\ $Id$

\ This file is part of Attila, a minimal threaded interpretive language
\ Copyright (c) 2007, UCD Dublin. All rights reserved.
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


\ ---------- Word status ----------

\ Compile the execution semantics of an immediate word
: POSTPONE \ ( "name" -- )
    ' COMPILE, ; IMMEDIATE

\ Grab the xt of the next word in the input at compile time and leave
\ it on the stack at run-time
: ['] \ ( "name" -- )
    ' POSTPONE LITERAL ; IMMEDIATE


\ ---------- Compilation ----------

\ At run-time, compile the next word in the input source at compile time
: [COMPILE] \ ( "word" -- )
    POSTPONE [']
    ['] COMPILE, COMPILE, ; IMMEDIATE

\ Compile an anonymous word with no name, leaving its xt on the data stack
: :NONAME \ ( -- xt )
    0 0 [ ' (:) >CFA @ ] LITERAL (HEADER,) DUP START-DEFINITION ] ;
    

\ ---------- Portable address arithmetic ----------

\ Compute a jump offset from a to TOP, in bytes
: JUMP> \ ( a -- offset )
    TOP SWAP - ;

\ Compute a jump offset from TOP to a, in bytes
: >JUMP \ ( a -- offset )
    TOP - ;


\ ---------- Recursion ----------

\ Call the current word recursively. This is needed because the currently-defined
\ word's name need not be present in the search order until the closing ;
: RECURSE \ ( -- )
    LASTXT COMPILE, ; IMMEDIATE


\ Call the current word tail-recursively. This call doesn't return here,
\ so use with care
\ TBD


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

