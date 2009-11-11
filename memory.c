// $Id: memory.c,v 1.4 2007/05/22 08:31:43 sd Exp $

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

// The memory model
//
// Most TILs have a concrete memory footprint. Attila has an abstract
// footprint to allow for experimentation.
//
// All Attila storage is organised into vocabularies. A vocabulary consists
// of memory set aside for headers, code and data, each of which may be of
// any appropriate size (including 0). The current vocabulary is used
// for accessing information such as the last-defined word and its header.
// Applications may create new vocabularies, either within the VM or at
// Attila level using vocabularies.fs
//
// Management of vocabularies is provided by functions in this file, which
// are then exposed as Attila words in the VM and the vocabularies sub-system.
// A system will have at least one vocabulary, and may have more if required.

#include "attila.h"


// The root vocabulary
VA rootvocabulary;

// User variables
static CELL user[USER_SIZE];


// ---------- Initialisation ----------

// Initialise a new vocabulary with the given partition sizes
VA
new_vocabulary( size_t ns, size_t cs, size_t ds, VA parent ) {
  VA voc = malloc(sizeof(VOCABULARY));
  
  voc->namespace_size = ns;   voc->head = voc->namespace = (byte *) calloc(ns, sizeof(byte));
  voc->codespace_size = cs;   voc->top = voc->codespace = (byte *) calloc(cs, sizeof(byte));
  voc->dataspace_size = ds;   voc->here = voc->dataspace = (byte *) calloc(ds, sizeof(byte));
  voc->ourvocabulary = parent;

  return voc;
}


// ---------- Allocation ----------
// sd: in all cases these words can be called with a 0 argument
// to get the current value of the underlying memory pointer

// Header memory
HA
allocate_word_header_memory( size_t n ) {
  VA voc = (VA) *(user_variable(USER_CURRENT));
  if((voc->head + n - voc->namespace) > voc->namespace_size)
    return 0;
  byte *ptr = voc->head;   voc->head += n;
  return ptr;
}


// Code memory
byte *
allocate_word_code_memory( size_t n ) {
  VA voc = (VA) *(user_variable(USER_CURRENT));
  if((voc->top + n - voc->codespace) > voc->codespace_size)
    return 0;
  byte *ptr = voc->top;   voc->top += n;
  return ptr;
}


// Data memory
byte *
allocate_word_data_memory( size_t n ) {
  VA voc = (VA) *(user_variable(USER_CURRENT));
  if((voc->here + n - voc->dataspace) > voc->dataspace_size)
    return 0;
  byte *ptr = voc->here;   voc->here += n;
  return ptr;
}


// ---------- User variables ----------

// Return the address of the i'th user variable, starting from 0
inline CELLPTR
user_variable( int i ) {
  return &user[i];
}
