// $Id$

// This file is part of Attila, a multi-targeted threaded interpreter
// Copyright (c) 2007, UCD Dublin. All rights reserved.
//
// Attila is free software; you can redistribute it and/or
// modify it under the terms of the GNU General Public License
// as published by the Free Software Foundation; either version 2
// of the License, or (at your option) any later version.
//
// Attila is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA 02111, USA.

// Attila initial dictionary
//
// Build the initial dictionary. These are the definitions that
// need to be (or are, at least :-) colon-definitions rather than
// primitives, but which can't be loaded as source. Primarily this
// is the colon-definition words themselves and the outer interpreter.
//
// After loading this file, the rest of the system is bootstrapped
// from TIL source code.

#include "bootstrap.h"


// ---------- The dictionary ----------

VOID
init_dictionary() {
  XTPTR startup;

  // ---------- Primitives ----------

  // compile all the primitives as words in the dictionary
  init_primitives();

  // ---------- Error handling ----------

  // abort with an error and restart the executive
  DEFINE("ABORT"); // ( addr len -- )
  COMPILE("TYPE");
  STRING(" ");
  COMPILE("TYPE");
  COMPILE("WARM");
  NEXT();


  // ---------- Compilation support ----------

  // return the xt of the last word defined
  DEFINE("LASTXT");
  LITERAL(USER_LAST);
  COMPILE("USERVAR");
  COMPILE("@");
  NEXT();

  // return the header address of the last word defined
  DEFINE("LASTHA");
  COMPILE("LASTXT");
  COMPILE(">HA");
  NEXT();

  
  // ---------- Compilation hooks and wrappers ----------
  // sd: these should realy be DEFERred words, but we don't necessarily
  // have DEFER and anyway we'd need FIND it make it work, so instead
  // we have the hooks as variables that are accessed by wrappers words

  // default START-DEFINITION behaviour
  // can be remapped later using ( xt ) (START-DEFINITION) !
  VARIABLE("(START-DEFINITION)");
  COMPILE_DATA_REFERENCE_TO("NOOP");

  // outer START-DEFINITION
  DEFINE("START-DEFINITION");
  COMPILE("(START-DEFINITION)");
  COMPILE("@");
  COMPILE("EXECUTE");
  NEXT();

  // default END-DEFINITION behaviour
  // can be remapped later using ( xt ) (END-DEFINITION) !
  VARIABLE("(END-DEFINITION)");
  COMPILE_DATA_REFERENCE_TO("NOOP");

  // outer END-DEFINITION
  DEFINE("END-DEFINITION");
  COMPILE("(END-DEFINITION)");
  COMPILE("@");
  COMPILE("EXECUTE");
  NEXT();

  // a flat find
  DEFINE("(FIND-IN-FLAT-NAMESPACE)");
  COMPILE("LASTHA");
  COMPILE("(FIND)");
  NEXT();

  // default FIND behaviour
  // can be remapped later using ( xt ) (FIND-POLICY) !
  VARIABLE("(FIND-POLICY)");
  COMPILE_DATA_REFERENCE_TO("(FIND-IN-FLAT-NAMESPACE)");

  // test for an IMMEDIATE word
  DEFINE("IMMEDIATE?");
  COMPILE(">STATUS");
  COMPILE("C@");
  LITERAL(STATUS_IMMEDIATE);
  COMPILE("AND");
  NEXT();

  // outer FIND
  // sd: not quite standard, as we don't use a counted string address 
  DEFINE("FIND"); // ( addr n -- addr n 0 | xt 1 | xt -1 )
  COMPILE("2DUP");                // addr n addr n
  COMPILE("(FIND-POLICY)");
  COMPILE("@");
  COMPILE("EXECUTE");             // addr n xt | 0
  COMPILE("DUP");
  COMPILE_IF(if9,th9,el10);
    COMPILE("ROT");
    COMPILE("2DROP");
    COMPILE("DUP");
    COMPILE("IMMEDIATE?");
    COMPILE_IF(if10,th10,el10);
      LITERAL(1);
    COMPILE_ELSE(if10,th10,el10);
      LITERAL(-1);
    COMPILE_ELSETHEN(if10,the0,el10);
  COMPILE_THEN(if9,th9,el9);
  NEXT();


  // ----- Compilation helpers -----

  // mask-in the given status mask onto a word
  DEFINE("SET-STATUS");
  COMPILE(">STATUS");
  COMPILE("DUP");
  COMPILE("C@");
  COMPILE("-ROT");
  COMPILE("OR");
  COMPILE("SWAP");
  COMPILE("C!");
  NEXT();

  // mark a word as immediate
  DEFINE("(IMMEDIATE)");
  LITERAL(STATUS_IMMEDIATE);
  COMPILE("SWAP");
  COMPILE("SET-STATUS");
  NEXT();

  // mark the previous word as immediate
  DEFINE("IMMEDIATE");
  COMPILE("LASTXT");
  COMPILE("(IMMEDIATE)");
  NEXT();

  // mark the previous word as having an indirect body field (CREATE'd)
  DEFINE("REDIRECTABLE");
  LITERAL(STATUS_REDIRECTABLE);
  COMPILE("LASTXT");
  COMPILE("SET-STATUS");
  NEXT();

  // test whether we're interpreting
  DEFINE("INTERPRETING?");  // ( -- interpret? )
  LITERAL(USER_STATE);
  COMPILE("USERVAR");
  COMPILE("@");
  LITERAL(STATE_INTERPRETING);
  COMPILE("=");
  NEXT();

  // find the xt of the next word in the input stream
  DEFINE("'"); // ( "word" -- xt )
  COMPILE("PARSE-WORD");
  COMPILE("FIND");
  COMPILE("0=");
  COMPILE_IF(if0,th0,el0);
    COMPILE("TYPE");
    STRING("?");
    COMPILE("ABORT");
  COMPILE_THEN(if0,th0,el0);
  NEXT();


  // ----- The colon-definition words -----

  // compile a literal
  DEFINE_IMMEDIATE("LITERAL");
  COMPILE_COMPILE("(LITERAL)");
  COMPILE("COMPILE,");
  NEXT();

  // compile a string literal
  DEFINE_IMMEDIATE("SLITERAL");
  COMPILE_COMPILE("(SLITERAL)");
  COMPILE("SCOMPILE,");
  NEXT();

  // compile a string from the input source
  DEFINE_IMMEDIATE("S\"");
  LITERAL(' ');
  COMPILE("CONSUME");
  LITERAL('\"');
  COMPILE("PARSE");
  COMPILE("?DUP");
  COMPILE_IF(if7,th7,el7);
    COMPILE("SLITERAL");
  COMPILE_ELSE(if7,th7,el7);
    STRING("String not delimited");
    COMPILE("ABORT");
  COMPILE_ELSETHEN(if7,th7,el7);
  NEXT();

  // force interpretation state
  DEFINE_IMMEDIATE("[");
  LITERAL(STATE_INTERPRETING);
  LITERAL(USER_STATE);
  COMPILE("USERVAR");
  COMPILE("!");
  NEXT();

  // force compilation state
  DEFINE("]");
  LITERAL(STATE_COMPILING);
  LITERAL(USER_STATE);
  COMPILE("USERVAR");
  COMPILE("!");
  NEXT();

  // the colon definer
  DEFINE(":");
  COMPILE("PARSE-WORD");
  LITERAL(&docolon);
  COMPILE("(WORD");
  COMPILE("START-DEFINITION");
  COMPILE("]");
  NEXT();

  // finish colon-definition
  DEFINE_IMMEDIATE(";");
  COMPILE_NEXT();
  COMPILE("END-DEFINITION");
  COMPILE("[");
  COMPILE("WORD)");
  NEXT();


  // ----- Outer interpreter -----

  // the outer interpreter, built using loops-by-hand
  DEFINE("OUTER"); // ( -- )
  COMPILE_BEGIN(be0,un0);
    COMPILE("PARSE-WORD");        // addr n
    COMPILE("?DUP");              // addr n n
    COMPILE("0<>");                // addr n f
    COMPILE_IF(if6,th6,el6);
      COMPILE("FIND");            // addr n 0 | xt f
      COMPILE_IF(if1,th1,el1);    // xt
        COMPILE("INTERPRETING?"); // xt interpret?
        COMPILE("OVER");          // xt interpret? xt
        COMPILE("IMMEDIATE?");    // xt interpret? immediate?
        COMPILE("OR");            // xt execute?
        COMPILE_IF(if2,th2,el2);  // xt
          COMPILE("EXECUTE");     //
	COMPILE_ELSE(if2,th2,el2);
          COMPILE("COMPILE,");    //
	COMPILE_ELSETHEN(if2,th2,el2);
      COMPILE_ELSE(if1,th1,el1);
        COMPILE("2DUP");          // addr n addr n
        COMPILE("NUMBER?");       // addr n v f
        COMPILE_IF(if3,th3,el3);
          COMPILE("ROT");         // v addr n
	  COMPILE("2DROP");       // v
          COMPILE("INTERPRETING?");  // v interpret?
          COMPILE("NOT");         // v compile?
          COMPILE_IF(if4,th4,el4);
	    COMPILE("LITERAL");
	  COMPILE_THEN(if4,th4,el4);
	COMPILE_ELSE(if3,th3,el3);          // addr n
          COMPILE("TYPE");        //
  	  STRING("?");
          COMPILE("ABORT");
	COMPILE_ELSETHEN(if3,th3,el3);
      COMPILE_ELSETHEN(if1,th1,el1);
    COMPILE_THEN(if6,th6,el6);
    COMPILE("EXHAUSTED?");
  COMPILE_UNTIL(be0,un0);
  NEXT();

  // cold-start the system
  // sd: not complete
  DEFINE("COLD");
  COMPILE("WARM");
  NEXT();


  // ----- Including files -----

  // load a file
  DEFINE("(LOAD)"); // fh --
  LITERAL(USER_INPUTSOURCE);    // fh 1
  COMPILE("USERVAR");           // fh input
  COMPILE("DUP");               // fh input input
  COMPILE("@");                 // fh input ofh
  LITERAL(2); COMPILE("PICK");  // fh input ofh fh
  LITERAL(2); COMPILE("PICK");  // fh input ofh fh input
  COMPILE("!");                 // fh input ofh

  // run executive in the original stack
  COMPILE(">R"); COMPILE(">R"); COMPILE(">R");
  LITERAL(USER_EXECUTIVE);
  COMPILE("USERVAR");
  COMPILE("@");
  COMPILE("EXECUTE");
  COMPILE("R>"); COMPILE("R>"); COMPILE("R>");

  COMPILE("-ROT");              // input ofh fh
  COMPILE("FILE-CLOSE");        // input ofh
  COMPILE("SWAP");              // ofh input
  COMPILE("!");                 //
  NEXT();

  // find a file and open it using the given file opener
  DEFINE("(OPEN-FILE)"); // ( addr len opener -- fh )
  COMPILE(">R");
  COMPILE("2DUP");                // addr len addr len
  COMPILE("R>");
  COMPILE("EXECUTE");             // addr len fh
  COMPILE("DUP");                 // addr len fh fh
  COMPILE_IF(if8,th8,el8);        // addr len fh
    COMPILE("ROT");               // fh addr len 
    COMPILE("2DROP");             // fh
  COMPILE_ELSE(if8,th8,el8);      // addr len fh
    COMPILE("DROP");              // addr len
    COMPILE("TYPE");              //
    STRING(" not accessible");
    COMPILE("ABORT");
  COMPILE_ELSETHEN(if8,th8,el8);
  NEXT();

  // load a file
  DEFINE("LOAD"); // ( addr len -- ) 
  COMPILE_REFERENCE_TO("FILE-OPEN");
  COMPILE("(OPEN-FILE)");
  COMPILE("(LOAD)");
  NEXT();

  // load a file -- same as LOAD
  DEFINE("INCLUDE"); // ( addr len -- ) 
  COMPILE("LOAD");
  NEXT();
   
  // compiler directive to include (load) a file
  DEFINE_IMMEDIATE("#INCLUDE");
  COMPILE("PARSE-WORD");
  COMPILE("INCLUDE");
  NEXT();

  // initialise for user input
  *(user_variable(USER_INPUTSOURCE)) = stdin;
  *(user_variable(USER_TIB)) = (CELL) malloc(TIB_SIZE);
  *(user_variable(USER_OFFSET)) = -1; // in need of a refill

  // put COLD into the start-up vector and set OUTER as the executive
  startup = (XTPTR) bottom;
  *startup = xt_of("COLD");
  *(user_variable(USER_EXECUTIVE)) = xt_of("OUTER");
}
