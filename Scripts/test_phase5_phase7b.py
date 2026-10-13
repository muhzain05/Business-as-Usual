"""Preserve original Phase 5 validator; fix only final-shelf contention's requested revision.

The final restock auto-returns the crate. A later ordinary E can legitimately collect an
order item, so two independent E packets need not share a revision. This version retains
both players' production E checks and pins the final contention pair to one revision.
"""
from pathlib import Path
source=Path(__file__).with_name('test_phase5_network.py').read_text()
old="        command('client','e');command('host','e')"
new="""        if sku=='crackers':
            command('client','raw',target=shelf,sequence=1001,revision=rev,order='S1-001')
            command('host','raw',target=shelf,sequence=1001,revision=rev,order='S1-001')
        else:
            command('client','e');command('host','e')"""
assert source.count(old)==1
source=source.replace(old,new).replace("check('client_local_E_restocks_'+sku,", "check(('client_local_E_restocks_' if sku=='tea' else 'client_fixed_revision_restocks_')+sku,")
exec(compile(source,str(Path(__file__).with_name('test_phase5_network.py')),'exec'))
