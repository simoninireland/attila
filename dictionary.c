// $Id: dictionary.c,v 1.5 2007/05/20 17:58:16 sd Exp $

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

// The layout of the dictionary
//
// Attila uses three memory partitions to store the different parts of
// words. The name space stores the word header including its name,
// status byte and link field. The code space stores the code of
// all non-primitive words, generally consisting of a sequence of
// execution tokens (xt's) to other words. The data space stores all
// program data.
//
// The rationale for this split is that the name space can be jetisoned
// for systems that are not interective and do not need to look-up the
// names of words at run-time. Thi is true for almost all embedded systems,
// as well as for traditional "compiled" applications. The code space
// is read-only, and so can be placed into ROM on an embedded system.
// The data space holds all mutable data. The cost of this arrangement
// is a couple of extra pointers for fully interactive systems, and a
// considerable space saving for embedded ones.
//
// A word header is laid out as follows:
//    1 byte     name length                       <-- ha (*1)
//    n bytes    name
//    1 cell     link field (lfa) to previous ha
//    1 cell     pointer to code space
//
// The code of a word consists of:
//    1 cell     code field (cfa)                  <-- xt (*2)
//    1 byte     status
//    1 cell     pointer to body
//    n cells    xt's of word
//
// See the VOCABULARY, CODE and HEADER structures defined in attila.h
//
// The code field pointer is the address of a C function (primitive), either
// a primitive or (in most cases) the inner interpreter for colon-definitions
// in which case the code block consists of execution tokens for words.
// An execution token (xt) is the address of the code field (marked *2 above).
//
// The body of the word is where its data is stored. Note that the xt
// and the body pointer, whilst the same in some implementations, are
// different in this one: care should be taken to always convert an xt
// into a body address using the >BODY word.
//
// Link fields point to the start of the header (ha) (*1 above). Note
// that there is no route from the xt to the ha, or indeed from the body
// to the xt: access is strictly top-down from name space to code space
// to data space.
//
// Most of the functions here are decorated as inline to allow for optimisation
// of address translation.

#include "attila.h"


// ---------- Compilation support ----------

// The xt is the address of the code segment
inline XT
ha_to_xt( HA ha ) {
  return ha->codeheader;
}

// Return the code segment header of an xt. This is simply a disguised cast
inline CODEPTR
xt_to_codeheader( XT xt ) {
  return (CODEPTR) xt;
}


// Return the address of a word's code, which is the first cell after
// the end of the code header
inline XT *
xt_to_code( XT xt ) {
  return (XT *) (((byte *) xt_to_codeheader(xt)) + sizeof(CODE));
}


// Return the address of a word's name, which is the first byte after the header
// or 0 if the word has no name
inline byte *
ha_to_name( HA ha ) {
  return (ha->namelen == 0) ? NULL : (((byte *) ha) + sizeof(HEADER));
}


// Return a pointer to the body of an xt, or 0 if it doesn't have one
inline CELLPTR
xt_to_body( XT xt ) {
  CODEPTR code = xt_to_codeheader(xt);
  CELLPTR ptr = code->body;

  // skip the indirect body field if present
  if(code->status & STATUS_REDIRECTABLE)
    ptr++;

  return ptr;
}


// Create a header for a word in the current vocabulary
HA
begin_word( byte *name, int namelen ) {
  // grab the current vocabulary
  VA voc = (VA) *user_variable(USER_CURRENT);
  HA lastha = voc->lastha;

  // set up the name header
  int headersize = sizeof(HEADER) + namelen;
  HA ha = allocate_word_header_memory(headersize);
  
  // fill-in the name
  ha->namelen = namelen;
  bcopy(name, ha_to_name(ha), namelen);
  
  // set up code header and body
  CODEPTR code = (CODEPTR) allocate_word_code_memory(sizeof(CODE));
  ha->lfa = lastha;
  ha->codeheader = code;
  ha->codeheader->cfa = (PRIMITIVE) &noop;
  code->body = allocate_word_data_memory(0);
  code->status = STATUS_DEFAULT;

  // update the vocabulary
  voc->lastha = ha;
  voc->lastxt = ha_to_xt(ha);

  return ha;
}


// Compile data into the code body of the current word being defined
byte *
compile_code( byte *addr, int len ) {
  byte *ptr = allocate_word_code_memory(len);
  bcopy(addr, ptr, len);
  return ptr;
}


// Compile a literal cell
inline CELLPTR
compile_cell( CELL i ) {
  return (CELLPTR) compile_code(&i, sizeof(CELL));
}


// Compile a literal byte
inline byte *
compile_byte( byte b ) {
  return compile_code(&b, sizeof(byte));
}


// Compile an xt
inline XT *
compile_xt( XT xt ) {
  return  (XT *) compile_code((byte *) &xt, sizeof(xt));
}


// Compile data into the data body of the current word being defined. The
// address may be null, in which case the memory is left un-initialised
byte *
compile_data( byte *addr, int len ) {
  byte *ptr = allocate_word_data_memory(len);
  if(addr != NULL)
    bcopy(addr, ptr, len);
  return ptr;
}


// Compile a single cell of data into data body of the current word body
inline CELLPTR
compile_data_cell( CELL c ) {
  return (CELLPTR) compile_data(&c, sizeof(CELL));
}


// Finish the definition of a word -- currently a noop
void
end_word() {
}
