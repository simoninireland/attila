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

\ Hooks run-time support
\
\ The words needed to run hooks. This allows hooks to be cross-compiled.

\ ---------- Hook construction ----------

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


\ ---------- User-facing words ----------
  
\ Run the words hung on a hook
: RUN-HOOK ( h -- f )
    BEGIN
	@ DUP 0<>
    WHILE
	    DUP >R
	    /CELL + @ EXECUTE
	    R> SWAP DUP 0<> IF
		NIP EXIT
	    ELSE
		DROP
	    THEN
    REPEAT ;
