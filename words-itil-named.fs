\ $Id$

\ This file is part of Attila, a retargetable threaded interpreter
\ Copyright (c) 2007--2011, Simon Dobson <simon.dobson@computer.org>.
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

\ Word headers, for indirected threading, with names.


\ ---------- Header creation ----------

\ Create a header for the named word with the given code field
: (WORD) \ ( addr n cf -- xt )
    OVER SEGMENT-QUANTUM + (CALLOT)  \ make room for a large-ish word, just in case
    SEGMENT-QUANTUM (ALLOT)          \ ...and its data
    CALIGNED                         \ align TOP on the next cell boundary 
    >R                               \ stash the code field
    DUP >R                           \ ...and the name length
    CSEQCOMPILE,
    CALIGNED      
    R> CCOMPILE,                     \ compile the name length
    0 CCOMPILE,                      \ status byte
    CALIGNED
    LASTXT ACOMPILE,                 \ compile the link pointer
    TOP                              \ the txt
    R> CFACOMPILE,                   \ the code pointer
    DUP LAST XT! ;                   \ update LAST


\ ---------- Header navigation ----------

\ Convert an xt to a name string. addr will be CALIGNED
: >NAME \ ( xt -- addr namelen )
    >LFA /CELL - DUP C@
    DUP >R
    -
    DUP /CELL /MOD DROP 0<> IF
        /CELL -
    THEN CALIGN                \ ensure we're aligned on the previous cell boundary
    R> ;


\ ---------- Basic finding ----------
\ sd: Hiding is incredibly ugly but incredibly useful for fine-tuning

\ Hide/unhide a word from the finder. Since a hidden word can't be found,
\ you need to stash its xt if you want to be able ti unhide it later
: (HIDE) ( xt -- )
    HIDDEN-MASK SWAP SET-STATUS ;
: (UNHIDE) ( xt -- )
    HIDDEN-MASK SWAP CLEAR-STATUS ;

\ Test whether a word is hidden
: HIDDEN? ( xt -- f )
    GET-STATUS HIDDEN-MASK AND 0<> ;

\ Test whether two word names match. This is just a case-insensitive
\ string comparison, but could be extended if desired
: NAMES-MATCH? \ ( addr1 len1 addr2 len2 -- f )
    2 PICK = IF
	SWAP
	BEGIN
	    ?DUP 0>
	WHILE
		-ROT
		OVER C@ >UC OVER C@ >UC C= IF
		    /CHAR + SWAP /CHAR +
		    ROT 1-
		ELSE
		    2DROP DROP FALSE EXIT
		THEN
	REPEAT
	2DROP TRUE
    ELSE
	DROP 2DROP FALSE
    THEN ;

\ Find the named word in the word list starting from the given
\ xt. Return 0 if the word can't be found, or it's xt and either
\ 1 for normal or -1 for IMMEDIATE words
: (FIND) ( addr n x -- xt 1 | xt -1 | 0 )
    BEGIN
	DUP 0<>
    WHILE
	    DUP HIDDEN? NOT IF
		DUP >NAME
		4 PICK 4 PICK NAMES-MATCH? IF
		    -ROT 2DROP
		    1 OVER IMMEDIATE? IF
			NEGATE
		    THEN
		    EXIT
		THEN
	    THEN
	    >LFA @
    REPEAT
    -ROT 2DROP ;
	    