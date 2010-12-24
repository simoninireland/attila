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
\ a particular "strategic" point -- for example when a file is loaded,
\ or when a word is defined -- and lets different parts of the system
\ specify some behaviour to be performed then.
\
\ Each word hung on a hook should have stack comment ( ... -- ... f ),
\ leaving a flag on the stack indicating whether the hook should keep
\ running words. A flag of TRUE stops execution. The flag returned by the
\ last word run is left on the stack after running the hook. This means
\ that hooks can be used to "try" operations, with the hook succeeding
\ as soon as a word returns TRUE. Hooked words are run oldest-to-youngest.
\
\ Hooks are dynamic, so the structure is maintained in data memory.

\ Create a hook with the given name
: (HOOK) ( addr n -- )
    (DATA)
    0 A, ;

\ Create a hook
: HOOK ( "name" -- )
    PARSE-WORD (HOOK) ;
    
\ Traverse a hook to the final link
: HOOK>LINK ( h -- addr )
    BEGIN
	DUP @ DUP 0<>
    WHILE
	    NIP
    REPEAT
    DROP ;  
    
\ Add a word to a hook
: ADD-TO-HOOK ( xt h -- )
    HOOK>LINK
    HERE SWAP A!  \ link previous link to HERE
    0 A,          \ store link
      A, ;        \ store xt of hooked word
  
\ Run the words hung on a hook
: RUN-HOOK ( h -- f )
    BEGIN
	@ DUP 0<>
    WHILE
	    DUP >R
	    1 CELLS + @ EXECUTE
	    R> SWAP DUP 0<> IF
		NIP EXIT
	    ELSE
		DROP
	    THEN
    REPEAT ;
