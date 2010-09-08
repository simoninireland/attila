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

\ Conditional compilation
\
\ This file provides [IF] ... [ELSE] ... [THEN] for conditional
\ compilation. Note that this construct cannot be nested, and
\ as a general rule should only appear at top level.

\ If the top of the stack is true, execute/compile text until the
\ next [ELSE] and jump to the next [THEN]; otherwise execute/compile the
\ text between the next [ELSE]...[THEN]
: [IF] ( f -- )
    NOT IF
	BEGIN
	    PARSE-WORD
	    2DUP S" [IF]" NAMES-MATCH? IF
		2DROP
		S" Nested [IF] not supported" ABORT
	    THEN
	    2DUP S" [ELSE]" NAMES-MATCH?
	    ROT  S" [THEN]" NAMES-MATCH? OR 
	UNTIL		
    THEN ; IMMEDIATE

\ Compile.execute the text until the next [THEN]
: [ELSE] ( -- )
    BEGIN
	PARSE-WORD
	2DUP S" [IF]" NAMES-MATCH? IF
	    2DROP
	    S" Nested [IF] not supported" ABORT
	THEN
	S" [THEN]" NAMES-MATCH?
    UNTIL ; IMMEDIATE

\ No-op (everything is done by [IF] and [ELSE])
: [THEN] ( -- ) ; IMMEDIATE

