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

\ Record tests

TESTCASES" Records"

TESTING" Record types"

RECORD: TESTREC
  1 CELLS FIELD: TESTREC-CELL
  1 CHARS FIELD: TESTREC-CHAR
;RECORD

{ ' TESTREC RECORDTYPESIZE 1 CELLS 1 CHARS + > -> TRUE }


TESTING" Instanciation and access"

TESTREC testrec1

{ testrec1 TESTREC-CELL DUP ALIGN = -> TRUE } 
{ testrec1 TESTREC-CHAR DUP ALIGN = -> TRUE } 

{ 1 testrec1 TESTREC-CELL !
  testrec1 TESTREC-CELL @ -> 1 }
{ 2 testrec1 TESTREC-CHAR C!
  testrec1 TESTREC-CHAR C@ -> 2 }
{ testrec1 RECORDSIZE ' TESTREC RECORDTYPESIZE = -> TRUE }


TESTING" Record type extension"

RECORD: TESTREC2 EXTENDS: TESTREC
  2 CELLS FIELD: TESTREC-DOUBLE
;RECORD

TESTREC2 testrec2

{ ' TESTREC2 RECORDTYPESIZE ' TESTREC RECORDTYPESIZE 2 CELLS + >= -> TRUE }

{ 1   testrec2 TESTREC-CELL !
  2   testrec2 TESTREC-CHAR C!
  0 3 testrec2 TESTREC-DOUBLE 2!

  testrec2 TESTREC-CELL @
  testrec2 TESTREC-CHAR @
  testrec2 TESTREC-DOUBLE 2@ -> 1 2 0 3  }


TESTING" Dynamic choice of record type"

{ HERE
  ' TESTREC RECORDTYPESIZE (CREATE-RECORD-DATA) = -> TRUE }
{ HERE
  ' TESTREC RECORDTYPESIZE (CREATE-RECORD-DATA) DROP
  HERE SWAP - -> ' TESTREC RECORDTYPESIZE }  
{ ' TESTREC RECORDTYPESIZE DUP (CREATE-RECORD-DATA) RECORDSIZE = -> TRUE }


