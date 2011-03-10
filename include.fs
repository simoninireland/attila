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
\ in turn until one is found.


\ ---------- Paths ----------

\ The base of the include paths chain
DATA INCLUDE-PATH 0 ,

\ The buffer for the full file path
DATA INCLUDE-FILENAME 256 ALLOT

\ Construct a file path from a chain component and a file stem
\ sd: this includes some pretty heroic stack manipulations it'd be
\ good to do without...
: MAKE-INCLUDE-FILENAME ( addr n addr' n' -- pathaddr pathn )
    2DUP INCLUDE-FILENAME                ( addr n paddr pn paddr pn fn )
    SWAP CMOVE                           ( addr n paddr pn )
    NIP                                  ( addr n pn )
    S" /"                 \ sd: should reflect system separator
    2DUP                                 ( addr n pn saddr sn saddr sn )
    4 PICK INCLUDE-FILENAME + SWAP CMOVE ( addr n pn saddr sn )
    NIP +                                ( addr n tn )
    ROT 2DUP                             ( tn addr n addr n )
    4 PICK INCLUDE-FILENAME + SWAP CMOVE ( tn addr n )
    NIP + INCLUDE-FILENAME SWAP ;

\ Traverse the chain concatenating the given file name with
\ the include path until we find one that exists, returning its
\ full path
: FIND-INCLUDE-FILE ( addr n -- addr' n' -1 | 0 )
    INCLUDE-PATH
    BEGIN
	NEXT-LINK-IN-CHAIN DUP 0<>
    WHILE
	    DUP >R
	    DATA-LINK-IN-CHAIN 2OVER MAKE-INCLUDE-FILENAME
	    R> ROT
	    2DUP READABLE? IF
		-ROT DROP 2SWAP 2DROP TRUE EXIT
	    ELSE
		2DROP
	    THEN
    REPEAT
    DROP 2DROP FALSE ;
 

\ ---------- Inclusion ----------

\ Find the given file either absolutely or via the include path chain
\ and load it if found
: INCLUDED \ ( addr len -- )
    \ try as-is first
    2DUP READABLE? IF
	\ found it, load it
	LOAD
    ELSE
	\ not available directly, try via inclusion
	2DUP FIND-INCLUDE-FILE IF
	    \ found it, load it
	    2SWAP 2DROP
	    LOAD
	ELSE
	    TYPE 32 EMIT ( SPACE ) S" not found in include path" ABORT
	THEN
    THEN ;

\ Include the following file
: INCLUDE \ ( "filename" -- )
    PARSE-WORD INCLUDED ;
