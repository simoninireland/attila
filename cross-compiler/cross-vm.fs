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

\ VM description generator for cross-compiler, C version

\ Translate characters unsafe for C
: SAFE-IDENTIFIER \ ( addr n -- )
    2DUP
    S" -><+@;:[]{}()!?/"
    S" ________________" TRANSLATE ;

\ Parse a word and make it a safe C identifier
: PARSE-IDENTIFIER \ ( "name" -- addr n )
    PARSE-WORD SAFE-IDENTIFIER ;

\ The defining words used in the vm and arch files
: CONSTANT \ ( n "name" -- )
    PARSE-IDENTIFIER
    ." #define " TYPE SPACE . CR ;
: USER \ ( n "name" -- )
    PARSE-IDENTIFIER
    ." #define USER_" TYPE SPACE . CR ;

\ C inclusions are inserted literally or by #include
: CHEADER: \ ( -- )
    BEGIN
	REFILL IF
	    SOURCE 1- S" ;CHEADER" S=CI? IF
		2DROP
		REFILL DROP
		FALSE
	    ELSE
		TRUE
	    THEN
	ELSE
	    S" Input ended before ;CHEADER" ABORT
	THEN
    WHILE
	    TYPE CR
    REPEAT ;

