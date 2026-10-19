# Is the Space Engineers VR plugin safe?

Oct 18, 2026 · @Mal

**We found nothing harmful.** Two AI reviewer agents went through the source code of [SpaceEngineersVR](https://github.com/ColbyJack1134/SpaceEngineersVR) separately, without seeing each other's conclusions, and agreed: the plugin has no way to reach the internet, never launches other programs, and only ever writes inside Space Engineers' own save folder. Version checked: **v1.1.5**, commit `0179423`.

**The exact code reviewed:** [SpaceEngineersVR at commit 0179423](https://github.com/ColbyJack1134/SpaceEngineersVR/tree/01794239d753b3b28e223dbaf09a8925d3682415) (tag `v1.1.5`). That link opens the tree as it stood when this was written, not the current one. If you are reading the latest code instead, the findings below may not apply to it.

**This is the second review.** The first covered v1.1.1. Every check here was run again from scratch against v1.1.5, and the mechanical results came back identical: still no networking code of its own, still the same 17 calls into Windows, still no code loaded at runtime. Two new features do change what is worth knowing, and both are covered below. The plugin now offers an API that other Workshop mods can read, and it can send your VR pose to programmable blocks.

## What we checked

A plugin runs inside Space Engineers with the same power as you do. There is no sandbox stopping it. So the question is not whether the code has bugs, it is whether the code can do anything outside its job, and whether it tries to.

There is a limited set of ways a Windows program can cause harm, and each one requires the code to use a specific, named feature. We searched for all of them.

| What we looked for | Why it matters | Found |
| --- | --- | --- |
| Internet access | Could send your data somewhere, or download more code | **None at all** |
| Running other programs | Could launch anything on your PC | **None** |
| Reading saved passwords or Steam login | Account theft | **None** |
| Recording what you type | Keylogging | **None** |
| Windows Registry changes | Making changes that survive uninstall | **None** |
| Hidden or scrambled code | A payload that only unpacks when it runs | **None** |
| Code loading itself at runtime | A way to bypass the checks above | **Two cases**, both harmless and shown below |
| Hidden text tricks in the source | Making code look different to a human than it does to the computer | **None** |
| Program files disguised as pictures | A common hiding place | **None**, all 66 images verified genuine |
| Code running during installation | Attacks that fire before you even play | **None** |

We also checked the parts that are harder to hide things in: what the installer downloads, where the code came from, who wrote it, and whether the project pulls in outside code libraries that could be swapped out. All clean. The project uses four well-known libraries, each recorded in a committed lock file so the same versions are fetched every build.

## What the plugin can do on your PC

It does have real powers, because a VR plugin needs them. Here is each one, what it is for, and what stops it being misused.

| It can | Because | What keeps it safe |
| --- | --- | --- |
| Press keys and click the mouse for you | So your VR controllers can use the game's flat 2D menus | Menu clicks and keystrokes only happen while Space Engineers is the active window, checked every time. Two things are deliberately not gated that way, and both are features: the media play/pause button on the desktop mirror (it is meant to control your music player), and a single Alt keypress used to pull the game back to the front when you squeeze a VR trigger, which by definition lands in whatever app is in front. It also moves your real mouse cursor and can scroll the wheel |
| See whether Shift, Ctrl, Alt or Windows is held | So it does not steal focus while you are mid-keystroke | It checks five keys and nothing else, and uses the answer to *stop* and do nothing. Nothing is recorded, saved, or sent. This is not keylogging |
| Mirror your desktop into VR | So you can see your monitor without taking the headset off | The capture is allocated on the graphics card with no CPU read access, and nothing in the plugin copies it back into memory, writes it to disk or sends it anywhere. See the privacy note below for what is captured |
| Load the SteamVR driver file | Required to talk to your headset | It gives Windows the complete exact path to the file in its own folder, so it cannot be tricked into picking up a file of the same name from somewhere else. Anyone who can write into the plugin's own folder could still replace it, so install from the official source |
| Send data to other players | Shows your VR arm and head movements to people in multiplayer | See below |
| Let other Workshop mods read your VR pose | So mods can make their own buttons and tools work in VR | Read-only, and only your headset position, hand positions and fingertip. A mod receives plain numbers and nothing it could use to reach your files or the network, so it gains no power it did not already have |
| Send your VR pose to programmable blocks | So in-game scripts can drive VR arms and custom cockpit controls | Only blocks you have renamed to contain `[SEVR]`, only on the ship you are piloting, and only ones you already have access to. See the privacy note below |
| Write files | Saves your settings, generated VR gunsights and shader files | Everything goes in `%AppData%\SpaceEngineers`, the game's normal save folder. Nothing is written into the game install or anywhere else on your PC |

### The multiplayer side

If you play online, this plugin accepts data sent by other players and the server. That is the one place where someone else's data enters the plugin, so we looked at it hardest.

It is written carefully. Every incoming message is size-checked before anything is done with it, and each kind of message is then accepted only at the exact lengths it should be. The sender's identity is authenticated by the game itself, and the plugin additionally refuses messages from players who should not be sending them, with rate limits per sender. In one spot the author wrote a custom routine specifically to avoid a known crash trick where an attacker claims a message is huge to exhaust your memory. That is not something you do by accident. One honest limit: for messages relayed through the host, the plugin trusts the host about who sent them, so you are trusting the server you chose to join

No message from another player can make the plugin touch your files, load code, or run anything. The worst a hostile host can do is put junk in the cockpit settings held in memory for that session.

### Privacy: what the desktop mirror actually shows

This is not a security flaw, but it is the thing most likely to catch you out, and it is a feature rather than a bug.

The desktop mirror captures **an entire monitor**, not just a window. By default it picks a screen that is *not* the one the game is running on, which for most people is the second monitor with everything else on it. Whatever is on that screen is visible inside your headset: chat messages, email, notifications, a password manager you left open.

It also matters that the mirror is drawn into the game's own picture. That means it can appear in **screenshots, recordings and livestreams** of Space Engineers. If you stream, treat the desktop mirror the way you would treat sharing your screen.

None of this sends anything anywhere. It is purely about what ends up on screen, and in anything capturing your screen.

### Privacy: your body movements are now readable inside the game

Two features added since v1.1.1 let other code in the game see how you are moving. Neither sends anything out of your PC, and neither can touch your files, but both are worth knowing about.

**Workshop mods can ask for your head and hand positions.** If a mod asks, the plugin hands it your headset position, your hand positions and your fingertip position. This is what lets a mod's own buttons work when you reach out in VR. What the mod gets is a handful of numbers. There is nothing in that exchange it could use to read your disk or reach the network, and Workshop mods are already fenced in by the game itself.

**Programmable blocks named `[SEVR]` are sent your posture, and this is switched on by default.** If you pilot a ship containing a programmable block whose name includes `[SEVR]`, the plugin sends that block's script your head, hand and finger positions roughly twenty times a second. The script sees your posture relative to the seat, not where you are in the world.

It is worth being clear about what that means in multiplayer: if you sit in someone else's ship and they have a programmable block named `[SEVR]`, their script receives your body movements. That is exactly what the feature is for, it only works on a block you already have access to, and the script can do nothing to your PC with it. If you would rather it did not happen at all, switch off the programmable block data setting in the VR settings.

### It rewrites parts of the game while it runs

All Space Engineers plugins do this, and it is the only way a mod like this can work, but it is worth understanding what you are agreeing to.

The plugin modifies roughly 50 of the game's own internal functions in memory as the game runs, covering rendering, camera, input, cockpits and building. Nothing is changed on disk and your game files are untouched. Practical consequences:

- **Crashes are your problem, not Keen's.** If the game breaks with the plugin loaded, that is not a supported configuration.
- **A game update can break it,** sometimes badly, because it hooks into internal functions that Keen is free to change. The plugin does check for this at startup and refuses to enable VR rendering if things do not match, which is the right behaviour.
- **Servers may not allow it.** Some server admins treat any code modification as cheating regardless of intent. Ask before joining a serious server with it.

## Two things worth knowing

Neither of these is a sign of anything bad. Both are worth understanding before you install.

**1. The review covered the source code, not the download.** We read the code on GitHub. The ready-made ZIP file on the releases page was built by the author on their machine, and there is no way to prove from the outside that it was built from exactly the code we read. This is normal and true of almost every mod you install, but it is worth saying plainly rather than letting the review sound like more than it is. If you want the strongest guarantee, build it yourself from the source. If you install the release like everyone else, you are trusting the author, which is the usual deal with mods.

**2. The headset driver file is not covered.** The plugin needs `openvr_api.dll` to talk to SteamVR. That file is Valve's, not the author's, and it is not in the source code we reviewed, so we cannot say anything about the copy that ships in the ZIP. In practice it is the standard Valve file. If you want to be thorough, compare it against the official copy from Valve's OpenVR release.

**For people building from source**, there is a third item: the project file switches on a built-in test harness by default. It is harmless and switched off unless you deliberately set special environment variables, but it is extra code you do not need, and as of v1.1.5 it also contains code that compiles a programmable block script, which is one more reason to leave it out. Building with `-p:DefineConstants=""` leaves it out.

## What this review does not cover

No code review catches everything, and you should know where the edges are rather than treating this as a clean bill of health for all time.

- **Only this version.** Everything here applies to v1.1.5, commit `0179423`. A later update could add anything. The check has to be repeated.
- **Not the ZIP, and not the Valve driver file.** Covered above.
- **Deliberately hidden flaws.** We were looking for a mod author doing something harmful, and we would have found it. We were not doing the kind of deep binary analysis that would catch a sophisticated attacker who planted a subtle memory-corruption bug designed to look like an innocent mistake. That level of attack is not realistic for a game mod, but it is not something we can rule out.
- **Nothing was run.** This review is reading, not testing. Nobody installed it in a sandbox and watched what it actually did.

## Check it yourself

Do not take this on trust. The whole point of writing it down is that you can repeat it.

The single most useful check, if the plugin ever worries you, is whether it can reach the internet. Clone the repository and run this:

```
git clone https://github.com/ColbyJack1134/SpaceEngineersVR
cd SpaceEngineersVR
git checkout 0179423

# 1. standard networking and process-launching classes
grep -rn --include="*.cs" -E "HttpClient|WebClient|WebRequest|Socket|TcpClient|UdpClient|Process.Start" .

# 2. the back door around check 1: direct calls into Windows itself
grep -rh --include="*.cs" -oE 'DllImport\("[^"]+"' . | sort | uniq -c

# 3. the other back door: code that picks what to run while it runs
grep -rn --include="*.cs" -E "Assembly.Load|Activator.CreateInstance|Type.GetType" . | grep -vE "/(Diagnostics|SEVR.Diagnostics)/"
```

Here is what you will actually see at commit `0179423`.

**Check 1 prints nothing.** No standard networking or process-launching code exists.

**Check 2 prints 17 lines**, all of them `user32.dll` except one `kernel32`. `user32` is the Windows window-and-input library, which is how the plugin drives menus and the cursor. The single `kernel32` line is the call that loads the SteamVR file. Nothing there reaches the network.

**Check 3 prints exactly two lines**, and both are harmless:

- `Patches/CursorClipPatch.cs:16` looks up a Space Engineers class by a name written out in full in the code. You can read the name; nothing is assembled at runtime.
- `Player/NativeGizmos.cs:29` creates an object of whatever type a game field already is. There is no name involved at all.

Seeing two results rather than zero is the normal outcome of an honest check. What matters is that both are fixed, readable, and not influenced by anything from outside the program. If a future version makes either of these build a name out of pieces, or load a file, that is worth a closer look.

All three checks matter together. Check 1 alone would not be enough, because a program can reach the network by calling Windows directly, or by assembling a class name while it runs so that no search for the obvious words finds it. Checks 2 and 3 close those two doors.

One clarification, since it sounds like a contradiction: the plugin *does* send data over a network when you play multiplayer. It sends your head and arm positions to other players, and it does that through Space Engineers' own multiplayer system, which is why none of the checks above pick it up. What the checks show is that it has no network connection of its own, separate from the game's.

When a new version comes out, you do not have to start over. Compare it against the version reviewed here and read only what changed:

```
git diff 0179423..HEAD -- "*.cs" "*.csproj" "*.ps1"
```

**A good extra step, for anyone who wants certainty rather than analysis:** install it in a throwaway virtual machine first, with a network capture running. If nothing leaves your PC while you play, that confirms the conclusion above by a completely different route.
