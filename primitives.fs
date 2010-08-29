\ $Id$

\ This file is part of Attila, a retargetable threaded interpreter
\ Copyright (c) 2007--2010, Simon Dobson <simon.dobson@computer.org>
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

\ Handle primitives when encountered in "normal" behaviour when
\ not cross-compiling.
\
\ Since we can't change the VM on the fly, we simply check
\ whether there's a word with the given name in the search
\ order and die if there isn't. This allows any normal words to
\ compile: it doesn't, of course, guarantee that the word we find
\ is the same as the primitive we expect.
\
\ See cross-primitives.fs for a cross-compiler view of primitives.

\ ---------- Primitive parser ----------

\ Parse a primitive and check for its existence at Attila level
: PRIMITIVE:
    PARSE-WORD 2DUP TYPE CR 2DUP FIND IF
	2DROP
    ELSE
	\ no word with this name, abort
	TYPE S" ?" ABORT
    THEN

    BEGIN
	REFILL IF
	    SOURCE 1- S" ;PRIMITIVE" S= IF
		REFILL DROP
		FALSE
	    ELSE
		TRUE
	    THEN
	ELSE
	    S" Input ended before ;PRIMITIVE" ABORT
	THEN
    WHILE
    REPEAT ;
