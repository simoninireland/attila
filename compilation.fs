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

\ Compilation helpers
\
\ These words are used to assist compilation. They may not be
\ needed on stand-alone embedded systems.

\ ---------- State management ----------

\ Enter interpretation state
: [ \ ( -- )
    INTERPRETATION-STATE STATE ! ; IMMEDIATE

\ Enter compilation state
: ] \ ( -- )
    COMPILATION-STATE STATE ! ;

\ Check whether we're interpreting
: INTERPRETING? \ ( -- f )
    STATE @ INTERPRETATION-STATE = ;


\ ---------- Word finding, compilation control and literals ----------

\ Flat finder, the default behaviour wrapped around whatever the underlying word list
\ management function is
:NONAME \ ( addr n -- 0 | xt 1 | xt -1 )
    LASTXT (FIND) ;

\ Hook, which consumes the unnamed xt generated above
DATA (FIND-BEHAVIOUR) XT, 

\ Top-level word-finder
: FIND \ ( addr n -- 0 | xt 1 | xt -1 )
    (FIND-BEHAVIOUR) XT@ EXECUTE ;
    
\ Look up a word, returning its xt
\ sd: depends on (ABORT) being as it is, not being a hook
: (') ( addr n -- xt )
    2DUP FIND 0= IF
	TYPE S" ?" ( ABORT ) (ABORT) @ EXECUTE
    ELSE
	-ROT 2DROP
    THEN ;

\ Look up the next word in the input stream and return its xt
: ' \ ( "name" -- xt )
    PARSE-WORD (') ;

\ Force compilation of the execution semantics of the next word,
\ whether it's IMMEDIATE or not 
: [COMPILE] \ ( "name" -- )
    ' CTCOMPILE, ; IMMEDIATE

\ Compile the xt on the top of the stack as a literal
: XTLITERAL \ ( xt -- )
    [ ' (LITERAL) CTCOMPILE,
      ' (LITERAL) XTCOMPILE, ]
    CTCOMPILE,
    XTCOMPILE, ; IMMEDIATE

\ Grab the xt of the next word in the input at compile time and leave
\ it on the stack at run-time
: ['] \ ( "name" -- )
    ' [COMPILE] XTLITERAL ; IMMEDIATE

\ Compile the compilation semantics of the next word
: POSTPONE \ ( "name" -- )
    ' DUP IMMEDIATE? IF
	\ immediate word, compile it into the current definition
	CTCOMPILE,
    ELSE
	\ non-immediate word, compile it when the current definition is executed
	[COMPILE] XTLITERAL
	['] CTCOMPILE, CTCOMPILE,
    THEN ; IMMEDIATE

\ Compile the value on the top of the stack as a literal
: LITERAL \ ( n -- )
    POSTPONE (LITERAL)
    COMPILE, ; IMMEDIATE

\ Compile the address on the top of the stack as a literal
: ALITERAL \ ( addr -- )
    POSTPONE (LITERAL)
    ACOMPILE, ; IMMEDIATE

\ Compile the string on the tp of the stack as a literal
: SLITERAL \ ( addr len -- )
    POSTPONE (SLITERAL)
    SCOMPILE, ; IMMEDIATE

\ Read a "-delimited string form the input and either leave it on the stack (when
\ interpreting) or compile it as a literal (when compiling). In interpretation mode
\ the string will be destroyed when a new line is read, so it needs to be used immediately
: S" \ ( "string" -- )
    32 CONSUME \ spaces
    [CHAR] " PARSE
    STATE @ INTERPRETATION-STATE = IF
	\ leave the string on the stack
    ELSE
	POSTPONE SLITERAL \ compile the string as a string literal
    THEN ; IMMEDIATE

\ Extract the first character of the next word in the input stream, leaving it
\ on the stack or compiling it as a literal
: [CHAR] \ ( "word" -- )
    PARSE-WORD DROP C@
    INTERPRETING? NOT IF
	POSTPONE LITERAL
    THEN ; IMMEDIATE


\ ---------- Recursion ----------

\ Call the current word recursively. This is needed because the currently-defined
\ word's name need not be present in the search order until the closing ;
: RECURSE \ ( -- )
    LASTXT CTCOMPILE, ; IMMEDIATE

\ Call the current word tail-recursively. This call doesn't return here,
\ so use with care
\ TBD
