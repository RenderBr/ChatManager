# ChatManager

This is a plugin for TShock for Terraria, by me. This plugin offers a set of features focussed on chat moderation & username checks.

## Permissions:

chatmanager.manage
- This permission will allow you to manage the filter dictionary.

chatmanager.ignore.chatfilter
- This permission will give you the ability to ignore the chat filter.

chatmanager.ignore.spamfilter
- This permission will give you the ability to ignore the spam timer.

## Supported exceptions:

A maximum of 20 characters are allowed in a name.

A maximum of (configurable) spacekeys are allowed in a name.

Only alphanumeric characters are allowed in a name, this can be dis- and enabled through config.

All items in the base filters are disallowed by default, you can make this offense banable by bool.

All items in the base filters are filtered out in chat. This does **NOT** remove the message, instead it replaces the word with a set of '***'

Limits messages to 1 per (configurable) amount of seconds. This prevents spam & inconvenient chat abuse.

All exception reasons are configurable.

## Config options:

```
{
  // Toggle the username filter.
  "ApplyNameValidator": true,
  
  // Ban or kick for inappropriate names.
  "BanIfInappropriateUsername": false,

  // Check for alphanumeric characters in names.
  "CheckForAlphaNumInUsername": true,

  // Check for space characters in names.
  "CheckMaxSpacesInUsername": true,

  // Sets the maximum allowed spaces in names.
  "MaxSpacesInUsername": 3,
  
  // Toggle the chatfilter.
  "ApplyChatFilter": true,

  // Interval in which messages are allowed to be sent in chat.
  "MsgSpamIntervalInSec": 5,

  // Custom error reasons, leave be if no change is needed.
  "DisconnectReasons": {
    "MaxLengthReason": "Your name is too long. Please rejoin with a shorter name. Maximum amount of characters is 20.",
    "MaxSpacesReason": "You have too many spaces in your name. The maximum allowed is {amountofspaces}. Please rejoin with a shorter name.",
    "InappNameReason": "Inappropriate names are not allowed. Please rejoin with a tolerable name.",
    "NonAlphaNumReason": "Foreign characters in your username are not allowed. Please rejoin with only alphanumeric characters.",
    "SpamMessageReason": "You are sending messages too fast. Please slow down."
  }
}
```

## Command:

/managefilter (or /mf) help/add/del/list

## Chat tags:

You can easily use colors in chat by applying the following parameters:

@color: MESSAGE \

Applicable tags are:

Red, Blue, Green, Yellow, Cyan, Pink, Purple.
Example:

`I am writing @red:a red message to display\ in chat.` < This will make the part from : to \ red.
