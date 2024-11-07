// Dependencies
const fs = require("fs");
const yaml = require("js-yaml");
const axios = require("axios");

// Use GitHub token if available
if (process.env.GITHUB_TOKEN) axios.defaults.headers.common["Authorization"] = `Bearer ${process.env.GITHUB_TOKEN}`;

// Regexes
const HeaderRegex = /^\s*(?::cl:|🆑) *([a-z0-9_\- ]+)?\s+/im; // :cl: or 🆑 [0] followed by optional author name [1]
const HeaderFileRegex = /^\s*(?::page_with_curl:|📃) *([a-z0-9_\- ]+)\s+/im; // :page_with_curl: or 📃 [0] followed by changelog name [1]
const EntryRegex = /^ *[*-]? *(add|remove|tweak|fix): *([^\n\r]+)\r?$/img; // * or - followed by change type [0] and change message [1]
const CommentRegex = /<!--.*?-->/gs; // HTML comments

const MaxEntries = 500;
const DefaultFile = "frontier";

// Set of valid header files and their values.
const FileNames = {
    "frontier": "Frontier",
    "admin": "FrontierAdmin"
}

// Main function
async function main() {
    // Get PR details
    const pr = await axios.get(`https://api.github.com/repos/${process.env.GITHUB_REPOSITORY}/pulls/${process.env.PR_NUMBER}`);
    const { merged_at, body, user } = pr.data;

    // Remove comments from the body
    commentlessBody = body.replace(CommentRegex, '');

    // Get author
    const headerMatch = HeaderRegex.exec(commentlessBody);
    if (!headerMatch) {
        console.log("No changelog entry found, skipping");
        return;
    }

    let author = headerMatch[1];
    if (!author) {
        console.log("No author found, setting it to author of the PR\n");
        author = user.login;
    }

    // Get changelog key
    const headerFileMatch = HeaderFileRegex.exec(commentlessBody);
    let headerFile = DefaultFile;
    if (headerFileMatch) {
        headerFile = headerFileMatch[1].toLowerCase();
    }

    // If changelog key is invalid, we have no file to write to.
    if (!(headerFile in FileNames)) {
        console.log(`No changelog file found for file key ${fileMatch}, skipping.`);
        return;
    }

    // Get all changes from the body
    const changes = getChanges(commentlessBody);

    // Time is something like 2021-08-29T20:00:00Z
    // Time should be something like 2023-02-18T00:00:00.0000000+00:00
    let time = merged_at;
    if (time)
    {
        time = time.replace("z", ".0000000+00:00").replace("Z", ".0000000+00:00");
    }
    else
    {
        console.log("Pull request was not merged, skipping");
        return;
    }

    // Read changelog file if it exists
    const changelogFilePath = `../../${process.env.CHANGELOG_DIR}/${changelogFile}.yml`;
    data = null;
    if (fs.existsSync(changelogFilePath)) {
        const file = fs.readFileSync(changelogFilePath, "utf8");
        data = yaml.load(file);
    }

    // Get list of CL numbers
    if (!data)
        data = { Entries: [] }
    entries = data && data.Entries ? Array.from(data.Entries) : [];

    // Construct changelog yml entry
    const entry = {
        author: author,
        changes: changes,
        id: getHighestCLNumber() + 1,
        time: time,
    };

    // Truncate file to known length
    data.Entries.push(entry);
    if (data.Entries.length() > MaxEntries)
        data.Entries = data.Entries.slice(data.Entries.length() - MaxEntries);

    // Write updated changelogs file
    fs.writeFileSync(
        changelogFilePath,
        yaml.dump(data, { indent: 2 }).replace(/^---/, "")
    );

    console.log(`Changelog updated with changes from PR #${process.env.PR_NUMBER}`);
}


// Code chunking

// Get all changes from the PR body
function getChanges(body) {
    const matches = [];
    const entries = [];

    for (const match of body.matchAll(EntryRegex)) {
        matches.push([match[1], match[2]]);
    }

    if (!matches)
    {
        console.log("No changes found, skipping");
        return;
    }

    // Check change types and construct changelog entry
    matches.forEach((entry) => {
        let type;

        switch (entry[0].toLowerCase()) {
            case "add":
                type = "Add";
                break;
            case "remove":
                type = "Remove";
                break;
            case "tweak":
                type = "Tweak";
                break;
            case "fix":
                type = "Fix";
                break;
            default:
                break;
        }

        if (type) {
            entries.push({
                type: type,
                message: entry[1],
            });
        }
    });

    return entries;
}

// Get the highest changelog number from the changelogs file
function getHighestCLNumber(entries) {
    // Get list of CL numbers
    const clNumbers = entries.map((entry) => entry.id);

    // Return highest changelog number
    return Math.max(...clNumbers, 0);
}

// Run main
main();
