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

\ Word headers, for indirected threading, with names.
\
\ The word header layout is as follows:
\
\           name       namelen bytes (CALIGNED)
\           namelen    1 byte        (CALIGNED)
\           status     1 byte
\    lfa -> link       1 cell
\     xt -> code       1 cell        (CALIGNED)
\  [ iba -> altbody    1 cell ]      (REDIRECTABLE words only)
\   body -> body       n bytes
\
\ Words are chained together and searched linearly.

\ ---------- Header access ----------

\ Convert an xt to the address of its code field (no-op in this model)
: >CFA ( xt -- xt ) ;

\ Manipulate the code field of a word
: CFA@ \ ( xt -- cf )
    >CFA @ ;
: CFA! \ ( cf xt -- )
    >CFA ! ;

\ Convert an xt to a link pointer containing the xt of the next word in the
\ definition chain
: >LFA \ ( xt -- lfa )
    /CELL - ;

\ Convert xt to its status field. The namelen and status are adjacent and
\ CALIGNED
: >STATUS \ ( xt -- addr )
    /CELL 2 * - 1+ ;

\ Convert an xt to a name string. addr will be CALIGNED
: >NAME \ ( xt -- addr namelen )
    >LFA /CELL - DUP C@
    DUP >R
    -
    DUP /CELL /MOD DROP 0<> IF
        /CELL -
    THEN CALIGN                \ ensure we're aligned on the previous cell boundary
    R> ;

\ Convert xt to indirect body (DOES> behaviour) if present
\ (We don't check whether there actually *is* an IBA)
: >IBA \ ( xt -- iba )
    /CELL + ;

\ Manipulate the IBA of a word
: IBA@ \ ( xt -- iba )
    >IBA @ ;
: IBA! \ ( addr xt -- )
    >IBA ! ;


\ ---------- Status and body management ----------

\ Mask-in the given mask to the status of a word
: SET-STATUS \ ( f xt -- )
    >STATUS DUP C@
    -ROT OR
    SWAP C! ;

\ Mask-out the given mask to the status of a word
: CLEAR-STATUS \ ( f xt -- )
    >STATUS DUP C@
    -ROT INVERT AND
    SWAP C! ;

\ Get the status of the given word
: GET-STATUS \ ( xt -- s )
    >STATUS C@ ;

\ Make the last word defined IMMEDIATE
: IMMEDIATE \ ( -- )
    IMMEDIATE-MASK LASTXT SET-STATUS ;

\ Test whether the given word is IMMEDIATE
: IMMEDIATE? \ ( xt -- f )
    GET-STATUS IMMEDIATE-MASK AND 0<> ; 

\ Make the last word defined REDIRECTABLE
: REDIRECTABLE \ ( -- )
    REDIRECTABLE-MASK LASTXT SET-STATUS ; 

\ Test whether the given word is REDIRECTABLE
: REDIRECTABLE? \ ( xt -- f )
    GET-STATUS REDIRECTABLE-MASK AND 0<> ; 

\ Convert xt to body address, accounting for iba if present
: >BODY \ ( xt -- addr )
    DUP >IBA
    SWAP REDIRECTABLE? IF
	/CELL +
    THEN ;


\ ---------- Header creation ----------

\ Create a header for the named word with the given code field
: (WORD) \ ( addr n cf -- xt )
    CALIGNED                 \ align TOP on the next cell boundary 
    >R                       \ stash the code field
    DUP >R                   \ ...and the name length
    CSEQCOMPILE,
    CALIGNED      
    R> CCOMPILE,             \ compile the name length
    0 CCOMPILE,              \ status byte
    CALIGNED
    LASTXT ACOMPILE,         \ compile the link pointer
    TOP                      \ the txt
    R> CFACOMPILE,           \ the code pointer
    DUP LAST XT! ;           \ update LAST


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
		ROT
		OVER C@ >UC OVER C@ >UC C= IF
		    /CHAR + SWAP /CHAR +
		    -ROT 1-
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
		    ROT 2DROP
		    1 OVER IMMEDIATE? IF
			NEGATE
		    THEN
		    EXIT
		THEN
	    THEN
	    >LFA @
    REPEAT
    ROT 2DROP ;
	    