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

\ Hooks
\
\ Hooks collect together a list of words that can be changed dynamically.
\ The idea is that a hook collects together the words that should happen
\ a particular "strategic" point -- for exaple when a file is loaded,
\ or when a word is defined -- and lets different parts of the system
\ specify some behaviour to be performed then.
\
\ Given the dynamic nature of a hook, hooked words shouldn't make
\ any assumptions about the order of their execution.
\
\ Hooks allocate a static amount of space for their hooked words, which
\ may be a little wasteful but is efficient. A linked-list representation
\ might save a little memory but would be more complicated to manage.

\ The number of words that can be hung from a hook -- modify with care
10 VALUE MAX-HOOKED-WORDS

\ Create a named hook
: (HOOK) \ ( addr n -- )
    (CREATE)
    MAX-HOOKED-WORDS DUP ,
    0 DO
	0 ,
    LOOP
    DOES>
      1 CELLS + ;

\ Create a hook from the next word
: HOOK \ ( "name" -- )
    PARSE-WORD (HOOK) ;

\ Add a word to a hook. The same word may appear more than once on a hook
: ADD-TO-HOOK \ ( xt addr -- ) 
    DUP 1 CELLS - @            \ xt addr n
    0 DO
	DUP @ 0= IF            \ xt addr
	    !
	    0 0 LEAVE
	ELSE
	    1 CELLS +
	THEN
    LOOP
    DROP 0<> IF
	S" Hook full" ABORT
    THEN ;

\ Remove a word from a hook. Removes only one instance, if there
\ are several
: REMOVE-FROM-HOOK \ ( xt addr -- )
    DUP 1 CELLS - @            \ xt addr n
    0 DO
	2DUP @ = IF            \ xt addr
	    0 SWAP !
	    0 LEAVE
	ELSE
	    1 CELLS +
	THEN
    LOOP
    2DROP ;

\ Run a hook
: RUN-HOOK \ ( addr -- )
    DUP 1 CELLS - @ 0 DO
	DUP @ ?DUP 0<> IF
	    EXECUTE
	THEN
	1 CELLS +
    LOOP
    DROP ;
