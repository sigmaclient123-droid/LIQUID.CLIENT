// api/data/data-manager.js
import { kv } from '@vercel/kv';

export async function handleDataManagement(req, res) {
    try {
        if (req.method !== 'POST') {
            return res.status(405).json({
                status: 405,
                error: 'MethodNotAllowed',
                message: 'You can only send POST requests to this URL'
            });
        }

        const authHeader = req.headers['authorization'];
        if (!authHeader || authHeader !== process.env.SECRET_KEY) {
            return res.status(401).json({ 
                'Unauthorized': 'To interact with any of the non static liquid APIs you must supply the secret key' 
            });
        }

        const body = req.body;
        const { action, ...params } = body;

        if (action !== 'increment_download') {
            if (!authHeader || authHeader !== process.env.SECRET_KEY) {
                return res.status(401).json({ 
                    'Unauthorized': 'To interact with any of the non static liquid APIs you must supply the secret key' 
                });
            }
        }

        if (!action) {
            return res.status(400).json({
                error: 'MissingRequiredField',
                message: 'Missing the required field: action'
            });
        }

        let currentData = await kv.get('liquid_data');

        if (!currentData) {
            currentData = {
                admins: [],
                superAdmins: [],
                liquidStatus: "online",
                status: "ONLINE", // For the download page
                "menu-version": "1.0.0", // For the download page
                "total-downloads": 0,
                "status-colors": { "ONLINE": "#00ff00", "OFFLINE": "#ff0000", "PATCHING": "#ffff00" },
                consoleStatuses: [],
                knownCheats: {},
                knownMods: {},
                knownPeople: {},
                modVersionInfo: [],
                modSpecificAdmins: [],
                specialCosmetics: {},
                specialCosmeticsDetailed: {},
                cleanUpForestObjectNames: []
            };
        }

        let result;
        switch (action) {
            case 'increment_download': result = incrementDownload(currentData); break;
            case 'add_admin': result = addAdmin(currentData, params); break;
            case 'remove_admin': result = removeAdmin(currentData, params); break;
            case 'add_superadmin': result = addSuperAdmin(currentData, params); break;
            case 'remove_superadmin': result = removeSuperAdmin(currentData, params); break;
            case 'change_liquid_status': result = changeLiquidStatus(currentData, params); break;
            case 'add_console_status': result = addConsoleStatus(currentData, params); break;
            case 'edit_console_status': result = editConsoleStatus(currentData, params); break;
            case 'remove_console_status': result = removeConsoleStatus(currentData, params); break;
            case 'add_known_cheat': result = addKnownCheat(currentData, params); break;
            case 'remove_known_cheat': result = removeKnownCheat(currentData, params); break;
            case 'add_known_mod': result = addKnownMod(currentData, params); break;
            case 'remove_known_mod': result = removeKnownMod(currentData, params); break;
            case 'add_known_person': result = addKnownPerson(currentData, params); break;
            case 'remove_known_person': result = removeKnownPerson(currentData, params); break;
            case 'update_motd': result = updateMotd(currentData, params); break;
            case 'update_version': result = updateVersion(currentData, params); break;
            case 'add_mod_version_info': result = addModVersionInfo(currentData, params); break;
            case 'edit_mod_version_info': result = editModVersionInfo(currentData, params); break;
            case 'remove_mod_version_info': result = removeModVersionInfo(currentData, params); break;
            case 'add_mod_specific_admin': result = addModSpecificAdmin(currentData, params); break;
            case 'remove_mod_specific_admin': result = removeModSpecificAdmin(currentData, params); break;
            case 'add_special_cosmetic': result = addSpecialCosmetic(currentData, params); break;
            case 'remove_special_cosmetic': result = removeSpecialCosmetic(currentData, params); break;
            case 'add_clean_up_forest_object_name': result = addCleanUpForestObjectName(currentData, params); break;
            case 'clear_clean_up_forest_object_names': result = clearCleanUpForestObjectNames(currentData); break;
            default:
                return res.status(400).json({ success: false, error: `Unknown action: ${action}` });
        }

        if (result.success) {
            await kv.set('liquid_data', currentData);
            return res.status(200).json(result);
        } else {
            return res.status(400).json(result);
        }

    } catch (error) {
        console.error(error);
        return res.status(500).json({ success: false, error: error.message });
    }
}

function incrementDownload(data) {
    data["total-downloads"] = (data["total-downloads"] || 0) + 1;
    return { success: true, message: 'Download incremented', current: data["total-downloads"] };
}

function updateVersion(data, { latest, minimum, menuVersion }) {
    if (latest) data.latestMenuVersion = latest;
    if (minimum) data.minimumMenuVersion = minimum;
    if (menuVersion) data["menu-version"] = menuVersion; // Updates the download page version
    return { success: true, message: 'Updated versions' };
}

function changeLiquidStatus(data, { status }) {
    if (!status) return { success: false, error: 'Missing status' };
    data.liquidStatus = status.toLowerCase();
    data.status = status.toUpperCase(); // Updates the download page status text
    return { success: true, message: `Changed liquid status to: ${status}` };
}


function addAdmin(data, { userId, name }) {
    if (!userId || !name) return { success: false, error: 'Missing userId or name' };
    if (data.admins.some(a => a.userId === userId)) return { success: false, error: 'Admin already exists' };
    data.admins.push({ name, userId });
    return { success: true, message: `Added admin: ${name}` };
}

function removeAdmin(data, { userId, name }) {
    const initialLength = data.admins.length;
    data.admins = data.admins.filter(a => (userId ? a.userId !== userId : a.name !== name));
    if (data.admins.length === initialLength) return { success: false, error: 'Admin not found' };
    return { success: true, message: `Removed admin` };
}

function addSuperAdmin(data, { name }) {
    if (!name) return { success: false, error: 'Missing name' };
    if (data.superAdmins.includes(name)) return { success: false, error: 'SuperAdmin already exists' };
    data.superAdmins.push(name);
    return { success: true, message: `Added superAdmin: ${name}` };
}

function removeSuperAdmin(data, { name }) {
    const initialLength = data.superAdmins.length;
    data.superAdmins = data.superAdmins.filter(a => a !== name);
    return data.superAdmins.length !== initialLength ? { success: true, message: 'Removed superAdmin' } : { success: false, error: 'Not found' };
}

function addConsoleStatus(data, { consoleName, status }) {
    if (data.consoleStatuses.some(cs => cs.consoleName === consoleName)) return { success: false, error: 'Exists' };
    data.consoleStatuses.push({ consoleName, status });
    return { success: true, message: 'Added console status' };
}

function editConsoleStatus(data, { consoleName, status }) {
    const cs = data.consoleStatuses.find(cs => cs.consoleName === consoleName);
    if (!cs) return { success: false, error: 'Not found' };
    cs.status = status;
    return { success: true, message: 'Updated' };
}

function removeConsoleStatus(data, { consoleName }) {
    data.consoleStatuses = data.consoleStatuses.filter(cs => cs.consoleName !== consoleName);
    return { success: true, message: 'Removed' };
}

function addKnownCheat(data, { key, value }) {
    data.knownCheats[key] = value;
    return { success: true, message: 'Added cheat' };
}

function removeKnownCheat(data, { key }) {
    delete data.knownCheats[key];
    return { success: true, message: 'Removed cheat' };
}

function addKnownMod(data, { key, value }) {
    data.knownMods[key] = value;
    return { success: true, message: 'Added mod' };
}

function removeKnownMod(data, { key }) {
    delete data.knownMods[key];
    return { success: true, message: 'Removed mod' };
}

function addKnownPerson(data, { userId, name }) {
    data.knownPeople[userId] = name;
    return { success: true, message: 'Added person' };
}

function removeKnownPerson(data, { userId }) {
    delete data.knownPeople[userId];
    return { success: true, message: 'Removed person' };
}

function updateMotd(data, { text }) {
    data.messageOfTheDayText = text;
    return { success: true, message: 'Updated MOTD' };
}

function addModVersionInfo(data, params) {
    data.modVersionInfo.push(params);
    return { success: true, message: 'Added mod version info' };
}

function editModVersionInfo(data, params) {
    const idx = data.modVersionInfo.findIndex(m => m.modName === params.modName);
    if (idx === -1) return { success: false, error: 'Not found' };
    data.modVersionInfo[idx] = params;
    return { success: true, message: 'Updated mod version' };
}

function removeModVersionInfo(data, { modName }) {
    data.modVersionInfo = data.modVersionInfo.filter(m => m.modName !== modName);
    return { success: true, message: 'Removed' };
}

function addModSpecificAdmin(data, { consoleName, userId, name, superAdmin }) {
    let msa = data.modSpecificAdmins.find(m => m.consoleName === consoleName);
    if (!msa) {
        msa = { consoleName, admins: [] };
        data.modSpecificAdmins.push(msa);
    }
    msa.admins.push({ name, userId, superAdmin });
    return { success: true, message: 'Added mod admin' };
}

function removeModSpecificAdmin(data, { consoleName, userId }) {
    const msa = data.modSpecificAdmins.find(m => m.consoleName === consoleName);
    if (msa) msa.admins = msa.admins.filter(a => a.userId !== userId);
    return { success: true, message: 'Removed' };
}

function addSpecialCosmetic(data, { cosmeticId, nonDetailedName, detailedName }) {
    data.specialCosmetics[cosmeticId] = nonDetailedName;
    data.specialCosmeticsDetailed[cosmeticId] = detailedName;
    return { success: true, message: 'Added cosmetic' };
}

function removeSpecialCosmetic(data, { cosmeticId }) {
    delete data.specialCosmetics[cosmeticId];
    delete data.specialCosmeticsDetailed[cosmeticId];
    return { success: true, message: 'Removed cosmetic' };
}

function addCleanUpForestObjectName(data, { objectName }) {
    if (!data.cleanUpForestObjectNames.includes(objectName)) {
        data.cleanUpForestObjectNames.push(objectName);
    }
    return { success: true, message: 'Added object' };
}

function clearCleanUpForestObjectNames(data) {
    data.cleanUpForestObjectNames = [];
    return { success: true, message: 'Cleared' };
}
