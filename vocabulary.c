// $Id: vocabulary.c,v 1.2 2007/05/20 17:51:12 sd Exp $

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

// Vocabulary creation and management primitives
//
// These words are pretty useless unless you also include the vocabularies.fs
// package to surface vocabularies at the Attila source level. They provide
// access to the vocabulary structure that manages Atilla-level within the
// virtual machine, but do not provide any real user-level functionality
// for creating vocabularies. In particular the creation of new word lists
// and their integration into the search order are provided on a non-primitive
// way. Systems that do not want to support user-level vocabularies do not
// need these features.
//
// Access to LASTHA and LASTXT are provided in vm.c as they are needed for
// bootstrapping purposes.

#include "attila.h"


// ---------- Creation ----------

// Return the identifier of the root vocabulary
// PRIMITIVE (ROOT-VOCABULARY) ( -- voc )
void
root() {
  CELL voc = rootvocabulary;
  PUSH_CELL(voc);
}


// Create a new vocabulary linked to the current one
// PRIMITIVE (NEW-VOCABULARY) ( -- voc )
void
bracket_new_vocabulary() {
  VA current = (VA) *(user_variable(USER_CURRENT));
  VA voc = new_vocabulary(NAMESPACE_MEMORY_SIZE, CODESPACE_MEMORY_SIZE, DATASPACE_MEMORY_SIZE, current);
  PUSH_CELL(voc);
}


// ---------- Control ----------

// Grab the relevant pointers needed to snapshot the vocabulary
// PRIMITIVE VOCABULARY-STATE@ ( voc -- vi-1 ... v0 i )
void
vocabulary_state_fetch() {
  VA voc;
  CELL i = 5;

  POP_CELL(voc);
  PUSH_CELL(voc->head);
  PUSH_CELL(voc->top);
  PUSH_CELL(voc->here);
  PUSH_CELL(voc->lastha);
  PUSH_CELL(voc->lastxt);
  PUSH_CELL(i);
}


// Restore a vocabulary to a state captured previously
// PRIMITIVE VOCABULARY-STATE! ( vi-1 ... v0 voc -- )
void
vocabulary_state_store() {
  VA voc;
  CELL v;

  POP_CELL(voc);
  POP_CELL(v);   voc->lastxt = v;
  POP_CELL(v);   voc->lastha = v;
  POP_CELL(v);   voc->here = v;
  POP_CELL(v);   voc->top = v;
  POP_CELL(v);   voc->head = v;
}
