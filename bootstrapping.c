// $Id: bootstrapping.c,v 1.3 2007/05/18 19:02:13 sd Exp $

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

// Support for image bootstrapping
//
// These routines are only used for bootstrapping an image. Once done,
// the compiler uses the Attila words directly.

#include "attila.h"


// ---------- Bootstrapping routines ----------

// Compile a primitive, returning its xt
XT
compile_primitive( char *name, PRIMITIVE prim, boolean immediate ) {
  HA ha = begin_word((byte *) name, strlen(name));

  // correct the primitive and status
  ha->codeheader->cfa = prim;
  if(immediate)
    ha->codeheader->status |= STATUS_IMMEDIATE;

  // return xt
  // printf("%s xt = %x, ha = %x, code = %x\n", name, ha_to_xt(ha), ha, xt_to_code(ha_to_xt(ha)));
  return ha_to_xt(ha);
}


// Begin a colon definition, returning its xt
inline XT
begin_colon_definition( char *name, boolean immediate ) {
  return compile_primitive(name, &inner_interpreter, immediate);
}


// End a colon definition
void
end_colon_definition() {
  end_word();
}


// Begin a data word (variable), returning its xt
inline XT
begin_variable( char *name ) {
  XT xt = compile_primitive(name, &do_var, 0);
  CODEPTR code = xt_to_codeheader(xt);
  code->body = allocate_word_data_memory(0);
  return xt;
}


// Return the xt of the given word
XT
xt_of_word( char *name ) {
  int namelen;
  VA voc = (VA) *(user_variable(USER_CURRENT));
  HA ha = voc->lastha;

  namelen = strlen(name);
  while(ha != NULL) {
    if(namelen == ha->namelen) {
      if(strncmp(name, ha_to_name(ha), ha->namelen) == 0)
	return ha_to_xt(ha);
    }
    ha = ha->lfa;
  }

  printf("%s?\n", name);
  return NULL;
}


// Compile the xt of a word into the current colon-definition, or NULL if
// the word is undefined
XT
compile_xt_of( char *name ) {
  XT xt = xt_of_word(name);
  // printf("Compiled %s %x\n", name, xt);
  return compile_xt(xt);
}


