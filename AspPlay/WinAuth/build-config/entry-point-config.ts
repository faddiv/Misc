import * as path from "path";
import * as fs from "fs";
import { Configuration, Entry } from "webpack";

function readFiles(filePath: string, pattern: RegExp): string[] {
    var entries: string[] = [];
    console.log("readFiles at ", filePath);
    const files = fs.readdirSync(filePath);
    console.log("Readed ", files.length, " files");
    for (const item of files) {
        const itemPath = path.join(filePath, item);
        console.log("Read ", itemPath);
        const stat = fs.statSync(itemPath);
        if (stat.isDirectory()) {
            for (const subFile of readFiles(itemPath, pattern)) {
                entries.push(subFile);
            }
        } else if (pattern.test(item)) {
            entries.push(itemPath);
        }
    }
    return entries;
}

export default function (pathBase: string, pattern: RegExp): Configuration {
    const searchFolder = path.resolve(pathBase);
    console.log("Parse entry points in ", searchFolder, " as ", pattern);
    let entry: Entry = {};
    for (let file of readFiles(searchFolder, pattern)) {
        const relativePath = path.relative(searchFolder, file);
        const entryName = relativePath.substring(0, relativePath.length - path.extname(file).length);
        console.log("Entry: ", entryName, ": ", relativePath);
        entry[entryName] = `.\\${pathBase}\\${relativePath}`;
    }
    return {
        entry
    };
}