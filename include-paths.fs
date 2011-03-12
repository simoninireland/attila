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

\ Source file include path manipulation
\
\ This file allows new paths to be added, which might not be desirable
\ for run-time use in some cases
\
\ Each path is a string, and so can't be longer than 256 characters; the
\ complete set of paths can be any length.

\ Add a path to the include paths chain
: ADD-INCLUDE-PATH ( addr n -- )
    INCLUDE-PATH ADD-LINK-TO-CHAIN ;

\ Add a colon-separated list of paths to the include path chain
: ADD-INCLUDE-PATHS ( addr n -- )
    [CHAR] : SPLIT
    BEGIN
	?DUP 0>
    WHILE
	    ROT ADD-INCLUDE-PATH
	    1-
    REPEAT ;

\ Display the curent include path as a :-separated string
: .INCLUDE-PATH ( -- )
    S" ." TYPE
    INCLUDE-PATH NEXT-LINK-IN-CHAIN
    BEGIN
	?DUP 0<>
    WHILE
	    S" :" TYPE
	    DUP DATA-LINK-IN-CHAIN TYPE
	    NEXT-LINK-IN-CHAIN
    REPEAT
    SPACE ;
