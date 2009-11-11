\ $Id: interpret-compile.fs,v 1.1 2007/05/25 13:13:27 sd Exp $

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

\ Structured compile-state-sensitive behaviours
\
\ Provide different behaviours when interpreting or compiling. This
\ is simply a structured way to address some common idioms which
\ switch depending on the compiler state.
\
\ The typical pattern of use is:
\
\ :NONAME \ the runtime behaviour
\     ... ;
\ :NONAME \ the compile- behaviour
\     ... ;
\ INTERPRET/COMPILE TOP-LEVEL-NAME
\
\ Then executing TOP-LEVEL-NAME will perform the correct operation
\ depending on the compiler state. You could of course use ' to
\ reuse an existing word.
\ Requires createdoes.fs

\ Build a state-dependent word
: INTERPRET/COMPILE \ ( rxt cxt "name" -- )
    CREATE
    SWAP , ,
    IMMEDIATE
  DOES> \ ( addr -- )
    INTERPRETING? NOT
    [ ' (?BRANCH) COMPILE, TOP 0 COMPILE, ]   \ IF
	CELL+
    [ DUP JUMP> SWAP ! ]                      \ THEN
    @ EXECUTE ;