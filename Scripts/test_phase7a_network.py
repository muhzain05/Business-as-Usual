"""Phase 7A: two independent Windows processes (actual NGO/UTP over localhost UDP).

Checks first-person ownership, viewmodel poses driven by authoritative state, collision in the
built game, local pause, real virtual-keyboard input isolation, settings persistence in the
player, live minimap tracking and leave-while-carrying recovery. Opt-in test placement is used to
reach stations; consequential actions use production E dispatch and held input. Automated, not a
human playtest.
"""
from pathlib import Path
import argparse, json, subprocess, time, traceback, sys, math

ROOT = Path(__file__).resolve().parents[1]
args = argparse.ArgumentParser()
args.add_argument('--output', type=Path, default=ROOT / 'Docs/Diagnostics/Phase7A/TwoProcess')
args.add_argument('--exe', type=Path, default=ROOT / 'Builds/Phase7A/BusinessAsUsual.exe')
args.add_argument('--width', type=int, default=1280)
args.add_argument('--height', type=int, default=720)
args.add_argument('--port', default='7787')
options = args.parse_args()
OUT = options.output.resolve(); OUT.mkdir(parents=True, exist_ok=True)
EXE = options.exe.resolve()
checks = []; processes = {}; serials = {'host': 0, 'client': 0}; last = {}; NOBODY = 18446744073709551615


def read(role):
    for _ in range(40):
        try:
            last[role] = json.loads((OUT / (role + '.json')).read_text()); return last[role]
        except (OSError, ValueError):
            time.sleep(.01)
    return last.get(role, {})


def wait(predicate, seconds=30):
    end = time.monotonic() + seconds
    while time.monotonic() < end:
        try:
            value = predicate()
            if value: return value
        except (KeyError, IndexError, TypeError):
            pass
        time.sleep(.08)
    raise AssertionError('Timed out waiting for network state')


def check(name, value, detail=''):
    checks.append(dict(name=name, passed=bool(value), detail=detail)); print(('PASS ' if value else 'FAIL ') + name + (' | ' + detail if detail else ''), flush=True)


def command(role, action, **kw):
    serials[role] += 1; p = OUT / (role + '.control.json'); tmp = p.with_suffix('.tmp')
    tmp.write_text(json.dumps(dict(serial=serials[role], action=action, **kw)))
    for _ in range(100):
        try: tmp.replace(p); break
        except PermissionError: time.sleep(.01)
    return serials[role]


def done(role, token): wait(lambda: read(role).get('serial') == token)


def launch(role):
    for suffix in ('json', 'control.json'): (OUT / (role + '.' + suffix)).unlink(missing_ok=True)
    processes[role] = subprocess.Popen([str(EXE), '-screen-fullscreen', '0', '-screen-width', str(options.width), '-screen-height', str(options.height), '-logFile', str(OUT / (role + '.log')),
                                        '--bau-role', role, '--bau-evidence', str(OUT / (role + '.json')), '--bau-control', str(OUT / (role + '.control.json')), '--bau-port', options.port], cwd=EXE.parent)


def state(): return read('host')['state']
def town(): return state()['town']
def fp(role): return read(role)['firstPerson']
def me(role): return next(p for p in read('host')['players'] if p['owner'] == read(role)['localId'])
def seen_by(viewer, role): return next(p for p in read(viewer)['players'] if p['owner'] == read(role)['localId'])
def synced(): wait(lambda: read('host')['state']['revision'] == read('client')['state']['revision']); time.sleep(.18)
def target(part): return next(s for s in read('host')['targetIds'] if s.endswith('/' + part) or s == part)
def point(part): p = read('host')['targetPoints'][read('host')['targetIds'].index(target(part))]; return p


def place(role, part=None, pos=None, yaw=0):
    if part == 'Parts Pickup Locker' and pos is None: pos = (-11.5, .08, 2.7); yaw = 90
    if pos is None:
        p = point(part); pos = (p['x'], .08, p['z'] - 1.1)
    token = command(role, 'teleport', x=pos[0], y=pos[1], z=pos[2], yaw=yaw); done(role, token); time.sleep(.45)


def use(role, part, local=False):
    synced(); rev = state()['revision']
    command(role, 'e' if local else 'interact', target=target(part) if not part.startswith('job:') and part not in ('return', 'drop') else part)
    wait(lambda: state()['revision'] > rev); synced()


def hold(role, value): token = command(role, 'hold', run=value); done(role, token); time.sleep(.15)


def capture(role, name, page=None):
    token = command(role, 'panel', run=page is not None, x=page or 0); done(role, token); time.sleep(.4)
    p = OUT / (role + '-' + name + '.png'); p.unlink(missing_ok=True); began = time.time()
    command(role, 'capture', target=name); wait(lambda: p.exists() and p.stat().st_mtime >= began); time.sleep(.25)


def look(role, yaw, pitch): token = command(role, 'look', x=yaw, y=pitch); done(role, token); time.sleep(.25)


def walk(role, start, yaw, seconds):
    place(role, pos=start, yaw=yaw); look(role, yaw, 0)
    command(role, 'move', x=0, y=1, yaw=yaw, duration=seconds, run=False); time.sleep(seconds + .6)
    return me(role)['position']


def key(role, name, duration=0):
    token = command(role, 'key', target=name, duration=duration); done(role, token); time.sleep(duration + .35)


def save(name):
    for role in ('host', 'client'): (OUT / (name + '-' + role + '.json')).write_text(json.dumps(read(role), indent=2))


try:
    launch('host'); wait(lambda: read('host').get('connected'), 60)
    launch('client'); wait(lambda: read('client').get('connected') and len(read('client').get('players', [])) == 2, 60); synced()

    # --- Ownership: one camera/listener/viewmodel per process; colleagues see the full body.
    for role in ('host', 'client'):
        r = read(role); f = r['firstPerson']
        check(role + '_one_camera_one_listener_one_input', r['cameras'] == 1 and r['listeners'] == 1 and r['inputs'] == 1, f"cameras {r['cameras']} listeners {r['listeners']} inputs {r['inputs']}")
        check(role + '_first_person_owner_only', f['active'] and f['hiddenBody'] and f['handsVisible'] and f['viewmodels'] == 1, json.dumps({k: f[k] for k in ('active', 'hiddenBody', 'handsVisible', 'viewmodels')}))
        other = 'client' if role == 'host' else 'host'
        check(role + '_sees_colleague_full_third_person_body', seen_by(role, other)['bodyVisible'] and not seen_by(role, role)['bodyVisible'])
        check(role + '_stable_eye_height', abs(f['cameraHeight'] - 1.0) < .06, f"{f['cameraHeight']:.3f}")
    capture('host', 'job-selection')
    use('host', 'job:2'); use('client', 'job:3')
    check('jobs_assigned_through_authority', town()['owners'][1] == read('host')['localId'] and town()['owners'][2] == read('client')['localId'])

    # --- Look direction replicates as public body/head orientation.
    place('host', pos=(0, .08, -1)); look('host', 90, -20); time.sleep(1.2)
    seen = seen_by('client', 'host')
    check('look_yaw_pitch_replicate_to_colleague', abs(((seen['lookYaw'] - 90) + 180) % 360 - 180) < 2 and abs(seen['lookPitch'] + 20) < 2, f"yaw {seen['lookYaw']:.1f} pitch {seen['lookPitch']:.1f}")
    check('idle_body_turns_to_face_look_direction', abs(((seen['facing'] - 90) + 180) % 360 - 180) < 8, f"facing {seen['facing']:.1f}")

    # --- Collision with production motor input in the built game.
    walks = [('market_facade_east_of_door', (-6.6, .08, 2.0), 0, 1.6, lambda p: p['z'] <= 3.0),
             ('market_door_closed', (-8.2, .08, 2.4), 0, 1.2, lambda p: p['z'] <= 3.3),
             ('garage_rear_tool_rack', (9.0, .08, 8.4), 0, 1.5, lambda p: p['z'] <= 8.9),
             ('van_side', (5.1, .08, 6.3), 90, 1.2, lambda p: p['x'] <= 5.5),
             ('yard_waste_cart', (11.15, .08, -6.2), 180, 1.2, lambda p: p['z'] >= -7.1),
             ('garage_opening_passable', (8, .08, 1.6), 0, 1.4, lambda p: p['z'] >= 4.0)]
    for name, start, yaw, seconds, ok in walks:
        end = walk('host', start, yaw, seconds)
        check('collide_' + name, ok(end) and end['y'] > -.2, f"end ({end['x']:.2f}, {end['y']:.2f}, {end['z']:.2f})")
    place('host', pos=(-6.75, .08, 2.0)); look('host', 0, 0); command('host', 'move', x=0, y=1, yaw=0, duration=1, run=False); time.sleep(1.4)
    capture('host', 'fp-pressed-against-market-wall')
    place('host', pos=(-8.2, .08, 1.2)); look('host', 0, 2); capture('host', 'fp-market-door-aligned')

    # --- Mechanic work with authoritative hand poses.
    place('host', 'Service Desk'); use('host', 'Service Desk', True)
    place('host', 'Vehicle 0 Inspection'); hold('host', True); use('host', 'Vehicle 0 Inspection', True)
    wait(lambda: fp('host')['handPose'] == 'inspect', 5); check('inspect_pose_follows_server_work', fp('host')['handPose'] == 'inspect')
    capture('host', 'fp-inspect')
    wait(lambda: town()['services'][0]['inspected'], 15); hold('host', False); synced()
    check('pose_returns_to_idle_after_work', wait(lambda: fp('host')['handPose'] == 'idle', 5))
    place('host', 'Vehicle 1 Inspection'); hold('host', True); use('host', 'Vehicle 1 Inspection', True); wait(lambda: town()['services'][1]['inspected'], 15); hold('host', False); synced()
    place('host', 'Parts Request Slip'); use('host', 'Parts Request Slip', True)
    place('host', 'Parts Pickup Locker'); use('host', 'Parts Pickup Locker', True)
    parts_before = list(town()['quantities'])
    place('host', 'Vehicle 0 Repair'); hold('host', True); use('host', 'Vehicle 0 Repair', True)
    wait(lambda: fp('host')['handPose'] == 'repair', 5); check('repair_pose_with_wrench_follows_server_work', fp('host')['handPose'] == 'repair')
    capture('host', 'fp-repair'); gestures = fp('host')['gestures']
    wait(lambda: town()['services'][0]['repaired'], 15); hold('host', False); synced()
    check('animation_never_double_consumes_parts', town()['quantities'][6] == 1 and town()['quantities'][4] == parts_before[4] - 1, str(town()['quantities']))
    place('host', 'Vehicle 1 Repair'); hold('host', True); use('host', 'Vehicle 1 Repair', True); wait(lambda: town()['services'][1]['repaired'], 15); hold('host', False); synced()
    place('host', 'Service Desk'); use('host', 'Service Desk', True); use('host', 'Service Desk', True)
    check('mechanic_shift_completes_in_first_person', town()['completed'][1] and len(town()['serviceRecords']) == 2)

    # --- Sanitation carry/sort poses on the client.
    place('client', 'Collection Route Desk'); use('client', 'Collection Route Desk', True)
    place('client', 'Route Bag 0'); use('client', 'Route Bag 0', True)
    wait(lambda: fp('client')['handPose'] == 'carry-bag', 5); check('carry_pose_grips_real_bag', fp('client')['handPose'] == 'carry-bag' and town()['bags'][0]['holder'] == read('client')['localId'])
    look('client', 0, 0); capture('client', 'fp-carry-bag')
    place('client', 'Paper Sorting Bin'); hold('client', True); use('client', 'Paper Sorting Bin', True)
    wait(lambda: fp('client')['handPose'] == 'sort', 5); check('sort_pose_follows_server_work', fp('client')['handPose'] == 'sort'); capture('client', 'fp-sort')
    wait(lambda: town()['bags'][0]['stage'] == 1, 15); hold('client', False); synced()
    check('sorted_once_no_duplicate', sum(1 for b in town()['bags'] if b['stage'] == 1) == 1)

    # --- Remote avatar: the colleague sees Sunny walk, run and jump in third person.
    place('host', pos=(-2, .08, -1.5), yaw=90); place('client', pos=(1, .08, -4.6), yaw=0); look('client', 0, 6)
    command('host', 'move', x=0, y=1, yaw=90, duration=1.2, run=False); time.sleep(.6)
    remote = seen_by('client', 'host'); capture('client', 'remote-host-walking')
    check('remote_walk_animates_from_replicated_speed', remote['speed'] > 1.5 and remote['grounded'], f"speed {remote['speed']:.2f}")
    time.sleep(1); place('host', pos=(1, .08, -1.5), yaw=0)
    command('host', 'move', x=0, y=0, yaw=0, duration=.6, run=False, jump=True); time.sleep(.22)
    airborne = seen_by('client', 'host'); capture('client', 'remote-host-jumping')
    check('remote_jump_replicates_airborne_state', not airborne['grounded'] or airborne['jumping'] or airborne['position']['y'] > .15, f"grounded {airborne['grounded']} y {airborne['position']['y']:.2f}")
    time.sleep(1); check('remote_lands_again', seen_by('client', 'host')['grounded'])

    # --- Live minimap: player tracking, objectives, colleague only when actually visible.
    place('client', pos=(2, .08, -1)); place('host', pos=(-2, .08, -1)); look('host', 90, 0); time.sleep(.6)
    check('colleague_marker_when_in_plain_view', fp('host')['visibleColleagues'] == 1, str(fp('host')['visibleColleagues']))
    place('client', pos=(8, .08, 8.6)); time.sleep(.6)
    check('no_colleague_marker_through_walls', fp('host')['visibleColleagues'] == 0, str(fp('host')['visibleColleagues']))
    check('minimap_baked_and_drawing_objectives', fp('host')['mapBaked'] and fp('host')['mapObjectives'] >= 0)
    token = command('host', 'map', run=True); done('host', token); time.sleep(.5)
    a = fp('host')['mapPlayer']; place('host', pos=(6, .08, -1), yaw=0); time.sleep(.5); b = fp('host')['mapPlayer']
    expected = (6 - (-2)) / 32
    check('expanded_map_tracks_real_position', abs((b['x'] - a['x']) - expected) < .01 and abs(b['y'] - a['y']) < .01, f"{a} -> {b}, expected dx {expected:.3f}")
    capture('host', 'map-expanded')
    token = command('host', 'map', run=False); done('host', token)
    capture('host', 'fp-hud-minimap')

    # --- Local pause with real Esc input; the colleague and authority keep running.
    place('client', pos=(-3, .08, -1)); place('host', pos=(3, .08, -1))
    # Production keyboard path from here: the harness stops synthesizing the host's movement.
    token = command('host', 'realinput', run=True); done('host', token)
    token = command('host', 'capturecursor'); done('host', token)
    key('host', 'Escape')
    check('escape_opens_local_pause', fp('host')['paused'] and not fp('host')['gameplayActive'] and fp('host')['inputBlocked'])
    before = seen_by('host', 'client')['position']
    command('client', 'move', x=0, y=1, yaw=90, duration=1, run=False); time.sleep(1.5)
    after = seen_by('host', 'client')['position']
    check('host_pause_does_not_freeze_colleague_or_server', math.hypot(after['x'] - before['x'], after['z'] - before['z']) > 1.4, f"moved {math.hypot(after['x'] - before['x'], after['z'] - before['z']):.2f} m")
    p0 = me('host')['position']; key('host', 'W', 1.0); p1 = me('host')['position']
    check('paused_player_ignores_real_movement_keys', math.hypot(p1['x'] - p0['x'], p1['z'] - p0['z']) < .05, f"moved {math.hypot(p1['x'] - p0['x'], p1['z'] - p0['z']):.3f} m")
    capture('host', 'pause-menu')
    token = command('host', 'pausepage', x=1); done('host', token); capture('host', 'pause-settings')
    token = command('host', 'pausepage', x=2); done('host', token); capture('host', 'pause-controls')
    token = command('host', 'pausepage', x=0); done('host', token)
    key('host', 'Escape')
    check('escape_resumes_and_relocks', not fp('host')['paused'] and fp('host')['gameplayActive'])
    p0 = me('host')['position']; key('host', 'W', 1.0); p1 = me('host')['position']
    check('resumed_player_moves_with_real_keys', math.hypot(p1['x'] - p0['x'], p1['z'] - p0['z']) > .8, f"moved {math.hypot(p1['x'] - p0['x'], p1['z'] - p0['z']):.2f} m")

    token = command('host', 'realinput', run=False); done('host', token)
    # --- Settings persist in the built player (restored to defaults afterwards).
    token = command('host', 'settings', x=.4, y=.25, z=60, run=False); done('host', token); time.sleep(.3)
    token = command('host', 'reloadsettings'); done('host', token); time.sleep(.3); f = fp('host')
    check('player_settings_persist_and_apply', abs(f['listenerVolume'] - .4) < .01 and abs(f['sensitivity'] - .25) < .001 and f['vSyncCount'] == 0 and f['targetFrameRate'] == 60, json.dumps({k: f[k] for k in ('listenerVolume', 'sensitivity', 'vSyncCount', 'targetFrameRate')}))
    token = command('host', 'settings', x=1, y=.12, z=0, run=True); done('host', token); time.sleep(.3); f = fp('host')
    check('vsync_option_applies', f['vSyncCount'] == 1 and f['targetFrameRate'] == -1)
    width, height = f['screenWidth'], f['screenHeight']
    token = command('host', 'display', x=1024, y=576, run=True); done('host', token); time.sleep(1.2); f = fp('host')
    check('windowed_resolution_option_applies', f['windowed'] and f['screenWidth'] == 1024 and f['screenHeight'] == 576, f"{f['screenWidth']}x{f['screenHeight']} windowed {f['windowed']}")
    token = command('host', 'display', x=width, y=height, run=True); done('host', token); time.sleep(1.2)

    # --- Leaving while carrying returns the load through the authority, then disconnects.
    place('client', 'Route Bag 1'); use('client', 'Route Bag 1', True)
    check('client_carrying_before_leave', town()['bags'][1]['holder'] == read('client')['localId'])
    command('client', 'leave'); wait(lambda: len(read('host')['players']) == 1, 20)
    check('leave_session_returns_carried_bag_and_role', town()['bags'][1]['holder'] == NOBODY and town()['bags'][1]['position'] == town()['bags'][1]['origin'] and town()['owners'][2] == NOBODY)
    save('completed')
    command('host', 'quit'); wait(lambda: processes['host'].poll() is not None, 20)
    check('host_quits_cleanly', True)
except Exception:
    try: save('failure')
    except Exception: pass
    traceback.print_exc(); checks.append(dict(name='unexpected_failure', passed=False, detail=traceback.format_exc()))
finally:
    for role, process in processes.items():
        if process.poll() is None:
            command(role, 'quit')
            try: process.wait(timeout=8)
            except subprocess.TimeoutExpired: process.terminate()
    report = dict(method='Two independent Windows processes, actual NGO/UTP localhost UDP, production E dispatch, held input and virtual-keyboard Esc/W, opt-in server test placement.', humanControlled=False, exe=str(EXE), checks=checks)
    (OUT / 'validation.json').write_text(json.dumps(report, indent=2)); print(f'{sum(c["passed"] for c in checks)}/{len(checks)} passed', flush=True); sys.exit(0 if all(c['passed'] for c in checks) else 1)
