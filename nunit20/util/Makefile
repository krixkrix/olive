thisdir = nunit20/util
SUBDIRS = 
include ../../build/rules.make

LIBRARY = NUnit.Util.dll
LIBRARY_NAME = nunit.util.dll
LIBRARY_SNK = $(topdir)/nunit.key

LOCAL_MCS_FLAGS= \
	/resource:Transform.resources,NUnit.Util.Transform.resources \
	-r:nunit.framework.dll -r:nunit.core.dll -r:System.dll \
	-r:System.Xml.dll /d:MONO /d:StronglyNamedAssembly
NO_TEST = yo

RESX_RES = Transform.resources

EXTRA_DISTFILES = \
	nunit.util.dll.csproj	\
	$(RESX_RES:.resources=.resx)

CLEAN_FILES = $(RESX_RES)

NO_INSTALL = yes

include ../../build/library.make

$(the_lib): $(RESX_RES)

$(RESX_RES): %.resources: %.resx
	$(RESGEN) `echo $< | $(PLATFORM_CHANGE_SEPARATOR_CMD)`
