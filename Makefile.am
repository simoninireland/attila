# $Id$

# This file is part of Attila, a multi-targeted threaded interpreter
# Copyright (c) 2007, UCD Dublin. All rights reserved.
#
# Attila is free software; you can redistribute it and/or
# modify it under the terms of the GNU General Public License
# as published by the Free Software Foundation; either version 2
# of the License, or (at your option) any later version.
#
# Attila is distributed in the hope that it will be useful,
# but WITHOUT ANY WARRANTY; without even the implied warranty of
# MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
# GNU General Public License for more details.
#
# You should have received a copy of the GNU General Public License
# along with this program; if not, write to the Free Software
# Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA 02111, USA.

# ----- Main program -----

bin_PROGRAMS = bootattila attila


# ----- Sources -----

# The bootstrapping sources
bootattila_SOURCES = vm.h \
	primgen.h \
	bootstrap.h \
	bootstrap.c \
	bootattila.c

# The cross-compiled sources
attila_SOURCES = x-host.h \
	x-host.fs

BUILT_SOURCES = primitives.c attila.c


# ----- The bootstrapped interpreter -----

# The collected bootstrap primitives, as a C file
nodist_bootattila_SOURCES = primitives.c

primitives.c: bootstrap.fs c-core.fs c-fileio.fs c-io.fs
	$(PERL) tools/primgen -o primitives.c $(srcdir)/bootstrap.fs $(srcdir)/c-core.fs $(srcdir)/c-fileio.fs $(srcdir)/c-io.fs


# ----- The cross-compiled interpreter -----

# The generated VM
nodist_attila_SOURCES = attila.c

attila.c: bootattila
	./bootattila <tools/crosscompile.fs

attila: attila.c


# ----- The system sources -----

# The standard prelude, included in a "real" Forth system
prelude = conditionals.fs \
	base.fs \
	createdoes.fs \
	interpret-compile.fs \
	variables.fs \
	constants.fs \
	values.fs \
	defer.fs \
	vm.fs \
	ascii.fs \
	bootstrap-loops.fs \
	stacks.fs \
	cs-stack.fs \
	loop-support.fs \
	loops.fs \
	counted-loops-runtime.fs \
	counted-loops.fs \
	case.fs \
	hooks.fs \
	chars.fs \
	strings.fs \
	zstrings.fs \
	scratch.fs \
	formatting.fs \
	lists.fs \
	file.fs \
	evaluate.fs \
	c.fs \
	wordlists.fs \
	dynamic-memory.fs

# The cross-compiler
crosscompiler = x-host.fs \
	x-host.h \
	c-image-fixedsize.fs \
	cross-compiler/cross-locator.fs \
	cross-compiler/cross-flat-memory-model.fs \
	cross-compiler/cross-c.fs \
	cross-compiler/cross-colon.fs \
	cross-compiler/cross-vm.fs \
	cross-compiler/cross-generate.fs \
	cross-compiler/cross-initialisation.fs \
	cross-compiler/cross-environments.fs

# The system sources, used to cross-compile a basic virtual machine
regular = c-core.fs \
	c-itil.fs \
	c-fileio.fs \
	c-io.fs \
	flat-memory-model.fs \
	itil-compilation.fs \
	compilation.fs \
	colon.fs \
	load.fs \
	executive.fs


# ----- Distribution -----

dist_pkgdata_DATA = $(prelude) $(crosscompiler) $(regular)
DISTCLEANFILES = primitives.c

