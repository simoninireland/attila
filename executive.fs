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

\ The standard outer executive
\ sd: this could be modularised to allow easier modification and hooking

\ ---------- Parsing words ----------

\ Hook for word parsers
HOOK PARSE-WORD-HOOK


\ ---------- Parsing numbers ----------



\ ---------- The executive ----------

\ The outer executive
: OUTER \ ( -- )
    BEGIN
	PARSE-WORD ?DUP IF
	    2DUP FIND ?DUP IF
		2SWAP 2DROP
		0< INTERPRETING? OR IF
		    EXECUTE
		ELSE
		    CTCOMPILE,
		THEN
	    ELSE
		2DUP NUMBER? IF
		    ROT 2DROP
		    INTERPRETING? NOT IF
			POSTPONE LITERAL
		    THEN
		ELSE
		    TYPE S" ?" ABORT
		THEN
	    THEN
	THEN
    EXHAUSTED? UNTIL ;
