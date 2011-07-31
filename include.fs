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

\ Source file inclusion, with include paths
\
\ Inclusion searches for the named file appended to each path
\ in turn until one is found. The user-given paths are searched
\ first, followed by the system paths. There are no user-level words
\ for changing the system paths, and this should be avoided: assume
\ that they're read-only, set by the cross-compiler. (There shouldn't
\ be any reason to touch them anyway.)

\ ---------- Paths ----------

\ The base of the system include paths chain
DATA SYSTEM-INCLUDE-PATH 0 ,

\ The base of the user-specified include paths chain
DATA INCLUDE-PATH 0 ,

\ The buffer for the full file path
DATA INCLUDE-FILENAME 256 ALLOT

\ Add a path component
: (ADD-INCLUDE-FILE-COMPONENT) ( addr n -- )
    INCLUDE-FILENAME /CHAR + DUP /CHAR - C@ 2SWAP 
    2OVER /CHAR * +
    SWAP DUP >R
    /CHAR * MOVE
    R> + SWAP /CHAR - C! ;

\ Construct a file path from a chain component and a file stem
: (MAKE-INCLUDE-FILENAME) ( addr n addr' n' -- pathaddr pathn )
    0 INCLUDE-FILENAME C!
    2SWAP (ADD-INCLUDE-FILE-COMPONENT)   \ the path
    S" /" (ADD-INCLUDE-FILE-COMPONENT)   \ separator -- sd: should reflect system path separator
          (ADD-INCLUDE-FILE-COMPONENT)   \ the file stem
    INCLUDE-FILENAME /CHAR + DUP /CHAR - C@ ; 
    
\ Traverse the chain concatenating the given file name with
\ the include path until we find one that exists, returning its
\ full path
: (FIND-INCLUDE-FILE) ( addr n link -- addr' n' -1 | 0 )
    BEGIN
	NEXT-LINK-IN-CHAIN DUP 0<>
    WHILE
	    DUP >R
	    DATA-LINK-IN-CHAIN 2OVER (MAKE-INCLUDE-FILENAME)
	    R> -ROT
	    2DUP READABLE? IF
		ROT DROP 2SWAP 2DROP TRUE EXIT
	    ELSE
		2DROP
	    THEN
    REPEAT
    DROP 2DROP FALSE ;

\ Traverse the user-supplied and system chains
: FIND-INCLUDE-FILE ( addr n -- addr' n' -1 | 0 )
    \ try as-is first
    2DUP READABLE? IF
	TRUE
    ELSE
	2DUP INCLUDE-PATH (FIND-INCLUDE-FILE) ?DUP 0<> IF
	    4 ROLL 4 ROLL 2DROP
	ELSE
	    SYSTEM-INCLUDE-PATH (FIND-INCLUDE-FILE)
	THEN
    THEN ;

\ Print the full path to a file to be included
: .INCLUDED ( addr n -- )
    2DUP FIND-INCLUDE-FILE IF
	2SWAP 2DROP TYPE
    ELSE
	TYPE 32 EMIT ( SPACE ) S" not found" TYPE
    THEN ;


\ ---------- Inclusion ----------

\ Find the given file via the include path chain and load it if found
: INCLUDED \ ( addr len -- )
    2DUP FIND-INCLUDE-FILE IF
	\ found it, load it
	2SWAP 2DROP
	LOAD
    ELSE
	TYPE 32 EMIT ( SPACE ) S" not found in include path" ABORT
    THEN ;

\ Include the following file
: INCLUDE \ ( "filename" -- )
    PARSE-WORD INCLUDED ; IMMEDIATE
