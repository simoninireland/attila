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

\ Forth unit testing, in the style of JUnit
\
\ Each test consists of a simple phrase of the form:
\
\ { <testing code> -> <results> }
\
\ When run, <testing code> is executed and compared against the results
\ of <results> (which may be literal values or another computation). Errors
\ are reported if the wrong number of arguments appear, or their values are
\ wrong.
\
\ This code is inspired by the work of John Hayes and colleagues of
\ Johns Hopkins University Applied Physics Laboratory. It has been
\ re-written and modified somewhat to meet modern testing practices.

\ ---------- Suppport ----------

\ The size of the largest set of stack results -- should be easily big enough
20 CONSTANT MAXIMUM-STACK-EFFECT

\ The size of safety margin we put on the stack before a test, to
\ (hopefully) stop destroying any data underneath -- should be enough
10 VALUE SAFETY-MARGIN

\ The stack cache to hold the results generated by the test code
DATA STACK-CACHE MAXIMUM-STACK-EFFECT CELLS ALLOT
VARIABLE INITIAL-STACK-DEPTH
VARIABLE ACTUAL-STACK-DEPTH
VARIABLE RESULT-STACK-DEPTH
VARIABLE TEST-CASE
0 TEST-CASE !

\ Make this value true for more verbose logging of tests
\ sd: We should do a general logging facility at system level
FALSE VALUE VERBOSE-LOGGING

\ Return the number of items deposited on the stack
: DEPOSITED ( -- n )
    DEPTH INITIAL-STACK-DEPTH @ - ;

\ Drop n values from the stack
: NDROP
    ?DUP 0> IF
	0 DO
	    DROP
	LOOP
    THEN ;

\ Alloate the safety margin
: ALLOCATE-SAFETY-MARGIN
    SAFETY-MARGIN 0 DO
	0
    LOOP ;


\ ---------- Individual test cases ----------

\ Start a test case
: { 
    ALLOCATE-SAFETY-MARGIN
    DEPTH INITIAL-STACK-DEPTH !
    1 TEST-CASE +!
    VERBOSE-LOGGING IF
	." Case #" TEST-CASE @ . CR
    THEN ;

\ Separate test from expected results, collecting the test results into the
\ stack cache for later comparison
: ->
    DEPOSITED DUP ACTUAL-STACK-DEPTH !
    ?DUP 0<> IF
	0 DO
	    STACK-CACHE I CELLS + !
	LOOP
    THEN ;

\ Compare test with expected results, reporting any discrepancies
: }
    DEPOSITED DUP RESULT-STACK-DEPTH !
    ACTUAL-STACK-DEPTH @ <> IF
	." Case #"                             TEST-CASE @ . SPACE
	." Wrong number of results: expected " RESULT-STACK-DEPTH @ .
	." , got "                             ACTUAL-STACK-DEPTH @ . CR
	RESULT-STACK-DEPTH @ SAFETY-MARGIN + NDROP
    ELSE
	ACTUAL-STACK-DEPTH @ ?DUP 0<> IF
	    0 DO
		STACK-CACHE I CELLS + @
		2DUP <> IF
		    SWAP
		    ." Case #"                 TEST-CASE @ . SPACE
		    ." Wrong value: expected " .
		    ." , got "                 . CR
		ELSE
		    2DROP
		THEN
	    LOOP
	THEN
	SAFETY-MARGIN NDROP
    THEN ;


\ ---------- Test suites ----------

\ Announce a new set of test cases
: TESTCASES" ( "comment" -- )
    BL CONSUME [CHAR] " PARSE
    CR ." ----- " TYPE SPACE ." -----" CR
    0 TEST-CASE ! ;

\ Announce a new test case
: TESTING" ( "comment" -- )
    BL CONSUME [CHAR] " PARSE
    TYPE CR
    0 TEST-CASE ! ;
 
\ Conduct the tests contained in the named file
: TESTCASES ( "name" -- )
    POSTPONE INCLUDE ;
