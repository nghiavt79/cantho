$ErrorActionPreference = "Stop"

$connectionString = "Server=localhost;Database=TechExchangeNew;User ID=sa;Password=111111;TrustServerCertificate=True;"

function New-Sha256Hash([string[]]$Parts) {
    $raw = [string]::Join([char]0x241F, ($Parts | ForEach-Object { if ($null -eq $_) { "" } else { $_ } }))
    $bytes = [System.Text.Encoding]::UTF8.GetBytes($raw)
    $sha = [System.Security.Cryptography.SHA256]::Create()
    try {
        $hash = $sha.ComputeHash($bytes)
    }
    finally {
        $sha.Dispose()
    }
    -join ($hash | ForEach-Object { $_.ToString("X2") })
}

function New-Slug([string]$Text) {
    if ([string]::IsNullOrWhiteSpace($Text)) { return "" }
    $normalized = $Text.Normalize([Text.NormalizationForm]::FormD)
    $sb = [System.Text.StringBuilder]::new()
    foreach ($ch in $normalized.ToCharArray()) {
        if ([Globalization.CharUnicodeInfo]::GetUnicodeCategory($ch) -ne [Globalization.UnicodeCategory]::NonSpacingMark) {
            [void]$sb.Append($ch)
        }
    }
    $s = $sb.ToString().Normalize([Text.NormalizationForm]::FormC).ToLowerInvariant()
    $s = [Text.RegularExpressions.Regex]::Replace($s, "[^a-z0-9\s-]", "")
    $s = [Text.RegularExpressions.Regex]::Replace($s, "\s+", "-")
    $s = [Text.RegularExpressions.Regex]::Replace($s, "-{2,}", "-").Trim("-")
    return $s
}

function Invoke-Scalar($Conn, [string]$Sql, [hashtable]$Params = @{}) {
    $cmd = $Conn.CreateCommand()
    $cmd.CommandText = $Sql
    foreach ($k in $Params.Keys) {
        [void]$cmd.Parameters.AddWithValue($k, $(if ($null -eq $Params[$k]) { [DBNull]::Value } else { $Params[$k] }))
    }
    return $cmd.ExecuteScalar()
}

function Get-Row($Conn, [string]$Sql, [hashtable]$Params) {
    $cmd = $Conn.CreateCommand()
    $cmd.CommandText = $Sql
    foreach ($k in $Params.Keys) {
        [void]$cmd.Parameters.AddWithValue($k, $(if ($null -eq $Params[$k]) { [DBNull]::Value } else { $Params[$k] }))
    }
    $reader = $cmd.ExecuteReader()
    try {
        if (-not $reader.Read()) { return $null }
        $row = @{}
        for ($i = 0; $i -lt $reader.FieldCount; $i++) {
            $name = $reader.GetName($i)
            $row[$name] = if ($reader.IsDBNull($i)) { $null } else { $reader.GetValue($i) }
        }
        return $row
    }
    finally {
        $reader.Close()
    }
}

function Copy-RowWithOverrides($Conn, [string]$Table, [string]$Pk, [int]$Id, [hashtable]$Overrides) {
    $exists = Invoke-Scalar $Conn "SELECT COUNT(1) FROM dbo.$Table WHERE OriginalId=@id AND LanguageId=2" @{ "@id" = $Id }
    if ([int]$exists -gt 0) { return @{ Created = $false; Id = $null } }

    $source = Get-Row $Conn "SELECT * FROM dbo.$Table WHERE $Pk=@id AND ISNULL(LanguageId,1)=1" @{ "@id" = $Id }
    if ($null -eq $source) { throw "Source row not found: $Table $Id" }

    $columns = @()
    $values = @()
    foreach ($name in $source.Keys | Sort-Object) {
        if ($name -eq $Pk) { continue }
        $columns += "[$name]"
        $values += "@$name"
    }

    $cmd = $Conn.CreateCommand()
    $cmd.CommandText = "INSERT INTO dbo.$Table ($([string]::Join(',', $columns))) VALUES ($([string]::Join(',', $values))); SELECT CAST(SCOPE_IDENTITY() AS int);"
    foreach ($name in $source.Keys | Sort-Object) {
        if ($name -eq $Pk) { continue }
        $value = if ($Overrides.ContainsKey($name)) { $Overrides[$name] } else { $source[$name] }
        [void]$cmd.Parameters.AddWithValue("@$name", $(if ($null -eq $value) { [DBNull]::Value } else { $value }))
    }
    $newId = [int]$cmd.ExecuteScalar()
    return @{ Created = $true; Id = $newId }
}

$productText = @{
    35394 = @{
        Name = "MISA eSign Digital Signature"
        QueryString = "misa-esign-digital-signature"
        MoTaNgan = "A remote signing solution that does not require a USB Token, allowing users to sign anytime, anywhere, on any device."
        MoTa = "MISA eSign allows digital signing directly on the web or mobile devices without a USB Token, with multi-level approval workflows, multi-factor authentication (OTP, mobile device and user account), centralized electronic storage and integration with other software systems such as e-invoice, HR and accounting platforms. The solution complies with Vietnamese regulations on electronic transactions and digital signatures, including Decree 130/2018/ND-CP."
        ThongSo = "Remote signing on smartphones, tablets and laptops; multi-factor OTP authentication; bulk signing for multiple documents; integration with e-invoice, HR and accounting systems."
        UuDiem = "Sign anytime, anywhere; flexible signing methods; secure smart storage; ready for scalable system integration; meets legal standards for identity authentication, data integrity and non-repudiation."
        XuatXu = "Domestic"
        TargetCustomer = "Businesses of all sizes, organizations, public administrative agencies, household businesses and individual businesses."
        GiaBanDuKien = '<table border="1"><tr><th>Package</th><th>Description</th><th>Estimated selling price</th></tr><tr><td>1 year</td><td>Digital signature package for household and individual businesses</td><td>1,050,000 VND</td></tr><tr><td>2 years</td><td>Digital signature package for household and individual businesses</td><td>1,450,000 VND</td></tr><tr><td>3 years</td><td>Digital signature package for household and individual businesses</td><td>1,750,000 VND</td></tr><tr><td>4 years</td><td>Digital signature package for household and individual businesses</td><td>2,050,000 VND</td></tr><tr><td>5 years</td><td>Digital signature package for household and individual businesses</td><td>1,850,000 VND</td></tr></table>'
        ChungNhanKhacText = "ISO 9001, ISO/IEC 27001, CMMI, CSA STAR; compliant with Decree 130/2018/ND-CP"
    }
    35396 = @{
        Name = "MISA meInvoice E-Invoice for Household and Individual Businesses"
        QueryString = "misa-meinvoice-e-invoice-for-household-and-individual-businesses"
        MoTaNgan = "A cloud-based e-invoice platform for household businesses, directly connected to the tax authority and integrated with MISA eSign."
        MoTa = "MISA meInvoice is an e-invoice platform that enables household businesses to create, issue, digitally sign, send, look up and store invoices in one system. It complies with Decree 70/2025/ND-CP and Circular 32/2025/TT-BTC. The platform supports e-invoices generated from cash registers connected directly to the tax authority, and integrates with MISA eSign and other platforms in the MISA ecosystem."
        ThongSo = "Cloud Computing platform; e-invoice issuance on computers, phones and tablets; direct connection to the tax authority; integration with MISA eSign."
        UuDiem = "Fully meets current legal requirements; simplifies the invoice creation, signing, issuance and sending process; automatically sends invoices to the tax authority; reduces printing, storage and delivery costs."
        TargetCustomer = "Household businesses and individual businesses."
        GiaBanDuKien = '<p><b>Invoices generated from cash registers:</b></p><table border="1"><tr><th>Package</th><th>Unit price</th><th>Total</th><th>Equivalent</th></tr><tr><td>5,000 cash-register invoices</td><td>995,000</td><td>995,000</td><td>199 VND/invoice</td></tr><tr><td>10,000 cash-register invoices</td><td>1,400,000</td><td>1,400,000</td><td>140 VND/invoice</td></tr><tr><td>20,000 cash-register invoices</td><td>2,200,000</td><td>2,200,000</td><td>110 VND/invoice</td></tr><tr><td>50,000 cash-register invoices</td><td>4,250,000</td><td>4,250,000</td><td>85 VND/invoice</td></tr><tr><td>100,000 cash-register invoices</td><td>7,000,000</td><td>7,000,000</td><td>70 VND/invoice</td></tr></table><p><b>Sales invoices:</b></p><table border="1"><tr><th>Package</th><th>Unit price</th><th>Promotion</th></tr><tr><td>300 invoices</td><td>950,000</td><td>-</td></tr><tr><td>1,000 invoices</td><td>1,550,000</td><td>Includes 1,000 free invoices</td></tr><tr><td>5,000 invoices</td><td>3,650,000</td><td>Includes 5,000 free invoices</td></tr></table>'
    }
    35465 = @{
        Name = "Business Management - Smart Sale for SME (1ERP)"
        QueryString = "business-management-smart-sale-for-sme-1erp"
        MoTaNgan = "An integrated enterprise resource planning (ERP) platform for small and medium-sized enterprises, supporting CRM, sales, purchasing, inventory, e-invoices and more."
        MoTa = "Smart Sale for SME (1ERP) is an integrated enterprise resource planning platform designed to help businesses manage and optimize their operations.`n`nWith Smart Sale for SME (1ERP), businesses can customize workflows to fit their specific needs and optimize purchasing, sales, customer service, human resource management and more.`n`nKey advantages:`n- Improved management efficiency: 1ERP acts as a connected hub across all aspects of enterprise management.`n- Better collaboration: the system consolidates business data and customer information, strengthening coordination between departments.`n- Effective planning capability: easy access to business information simplifies analysis.`n- Scalability and flexibility: the modular structure can be adjusted to match business growth.`n`nMore information: https://it.mobifone.vn/giai_phap/1erp - Introductory video: https://www.youtube.com/watch?v=9XDo-1ZdNrI"
        ThongSo = "Solution features:`n1. Customer Relationship Management - CRM: automatically collects data from channels and updates customer information management.`n2. Sales Management: manages quotations, products, order details and receivables.`n3. Purchasing Management: manages the purchasing process from order creation and supplier management to order processing.`n4. Inventory Management: manages inventory and product locations.`n5. E-Invoice Management: integrated with MobiFone Invoice.`n6. Landing page builder for businesses.`n7. Internal/public forum, live chat and blog.`n8. Recruitment: manages the recruitment workflow.`n`nReference packages: from 2,400,000 VND (User ES package, 5 users / 6 months) to 14,400,000 VND (SCM package, 10 users / 12 months). Contact the supplier for advice on the appropriate package."
        UuDiem = "Improves management efficiency; enhances collaboration between departments; supports effective planning; scalable and flexible thanks to a modular structure."
        Keywords = "ERP, enterprise management, CRM, sales management, MobiFone, 1ERP, SME, Smart Sale"
        TargetCustomer = "Small and medium-sized enterprises (SMEs)"
    }
    35587 = @{
        Name = "Clean Sand Screening, Washing and Treatment System and Technology (sea sand, river sand and saline sand)"
        QueryString = "clean-sand-screening-washing-and-treatment-system-and-technology"
        MoTa = '<p>Includes 2 granted patents:</p><table border="1" cellpadding="4" cellspacing="0"><tr><th>Invention name</th><th>Patent number</th><th>Decision number</th><th>Grant date</th></tr><tr><td>System and method for screening, washing and classifying saline sand</td><td>35015</td><td>1907w/QD-SHTT</td><td>17/02/2023</td></tr><tr><td>Process for washing fine aggregates for construction</td><td>42793</td><td>154666/QD-SHTT</td><td>26/12/2024</td></tr></table><p><b>Context and problem addressed:</b> Investment in sea-sand screening, washing and treatment stations aims to produce clean sand that meets technical requirements for concrete, construction mortar, road foundations and ground filling, replacing increasingly scarce river sand. Vietnam has an estimated 195 billion cubic meters of sea sand resources, but natural sea sand often contains mud, dust, clay, organic impurities, soluble salts and chloride ions, so it must be washed and treated to meet TCVN 13754:2023, TCCS 49:2025/CDBVN and TCVN 9436:2012. The technology can also utilize sand from dredging channels, estuaries and seaports, helping reduce dredging and disposal costs while adding construction material supply.</p><p><b>Economic and social benefits:</b> Reduces cement used in mixing by 10%-17% (Can Tho University research project, 2014); increases concrete strength by 10%-20% (Quatest 3 laboratory technician guide, 2007); controls Cl- ions to meet road embankment standards issued by the Ministry of Transport (&lt;0.01% for prestressed concrete, &lt;0.05% for ordinary concrete).</p>'
        ThongSo = '<p><b>TECHNICAL SPECIFICATIONS AND CAPACITY OF THE SEA-SAND SCREENING AND WASHING SYSTEM</b></p><table border="1" cellpadding="4" cellspacing="0"><tr><th>Item</th><th>Specification</th></tr><tr><td>Design capacity</td><td>150 m3/hour (raw sand, operating 9 months/year); can be designed up to 1,000 m3/hour as required</td></tr><tr><td>Production capacity</td><td>1 shift/day: 259,200 m3/year - 2 shifts/day: 518,400 m3/year</td></tr><tr><td>Power consumption</td><td>Total capacity 100 HP (recommended transformer station >= 150 KVA)</td></tr><tr><td>Freshwater demand</td><td>4 m3 water / 1 m3 raw sand (surface water from rivers and canals)</td></tr><tr><td>Installation area + finished product yard</td><td>>= 0.8 ha</td></tr><tr><td>Settling pond for water and impurities</td><td>>= 1.5 ha</td></tr><tr><td>Wastewater to be treated</td><td>4,800 m3/shift (8 hours) - total 9,600 m3/day (2 shifts), treated through settling ponds before discharge</td></tr><tr><td>Quality after treatment</td><td>Organic impurities &lt; 1%, chloride ions (Cl-) &lt; 0.01%</td></tr><tr><td>Input materials</td><td>Hill sand, mountain sand, river sand, stream sand, sea sand and saline sand</td></tr><tr><td>Applicable standards</td><td>TCVN 13754:2023 (saline sand for concrete and mortar); TCCS 49:2025/CDBVN (road foundations using sea sand); TCVN 9436:2012 (sand for filling)</td></tr></table>'
        UuDiem = '<ul><li>The sand screening and washing equipment system has won domestic and international science awards and received a Certificate of Merit from the Prime Minister for contributions to research and development of technologies applied in real production.</li><li>Industrial-scale production, with capacity of 100-500 m3/hour/equipment and expandable up to 2,000 m3/hour/equipment when required, meeting large-scale exploitation needs at industrial parks, seaports, sand mines and key projects.</li><li>Domestic production with a more reasonable cost than imported equipment, optimizing investment costs while maintaining quality.</li><li>Highly automated, easy to operate, and uses no chemicals in the sand washing process (only freshwater circulated through multiple settling and filtration basins before discharge), making it an environmentally friendly solution aligned with green and sustainable economic development.</li></ul>'
        TargetCustomer = "Organizations and individuals exploiting or trading sand who need technology transfer, transfer of technology use rights, services for screening and washing natural or crushed sand into fine aggregates (clean sand for concrete, plastering mortar and filling), or partners who need system installation and equipment transfer."
    }
    35395 = @{
        Name = "China Ecotek Industrial LED Lights (BV120B / BV240)"
        QueryString = "china-ecotek-industrial-led-lights-bv120b-bv240"
        MoTa = "120W screw-base LED light (BV120B): 1KV surge-protected AC IC circuit, ceramic COB chip, standard E40 base for direct replacement of old mercury lamps. 240W LED High Bay (BV240): ceramic COB superconductive plate, non-aging optical lens, patented integrated heat dissipation system, suitable for dusty, hot and humid environments."
        ThongSo = '<table border="1"><tr><th>Specification</th><th>BV120B (120W)</th><th>BV240 (240W)</th></tr><tr><td>Light source</td><td>Ceramic COB chip</td><td>Ceramic COB chip</td></tr><tr><td>Beam angle</td><td>60/90/120 degrees</td><td>25/60/90/120 degrees</td></tr><tr><td>Color temperature</td><td>3000K/4500K/5500K</td><td>3000K/4500K/5500K</td></tr><tr><td>Brightness</td><td>>=120Lm/W</td><td>>=120Lm/W</td></tr><tr><td>Color rendering index</td><td>80Ra</td><td>80Ra</td></tr><tr><td>Power supply</td><td>IC Driver AC 220V</td><td>MEAN WELL AC 220V/90-305V/180-520V</td></tr><tr><td>Operating environment</td><td>-25C~50C</td><td>-25C~50C</td></tr><tr><td>Protection rating</td><td>-</td><td>IP68 (power supply IP65)</td></tr><tr><td>Connector base</td><td>E40</td><td>-</td></tr><tr><td>Weight</td><td>1.2Kg</td><td>4.3Kg</td></tr></table>'
        UuDiem = "1KV surge protection; concentrated COB light source with low thermal resistance coefficient; integrated heat dissipation; lightweight design; BV240 meets IP68 standards for harsh environments."
    }
    35397 = @{
        Name = "Fluidized Bed Boiler"
        QueryString = "fluidized-bed-boiler"
        MoTaNgan = "Fluidized Bed Boiler with 5-50 tons/hour capacity, multi-fuel biomass combustion and efficiency up to 88%."
        MoTa = "The system operates on the principle of burning fuel in a bed of inert particles (quartz sand) kept suspended by a high-pressure compressed air flow, forming a fluidized state. Fuel fed into the combustion chamber ignites immediately at the high temperature of the fluidized sand layer, transferring heat to water-tube walls to generate steam. Flue gas passes through heat recovery systems (economizer and air preheater) and multi-stage treatment systems (multi-cyclone, bag filter/wet scrubber tower) before exiting through the chimney."
        ThongSo = "Boiler type: Fluidized Bed Boiler; steam capacity: 5-50 tons/hour; design pressure: below 50 bar; boiler efficiency: 85-88% when integrated with Eco heat economizer and air preheater; fuel: multi-fuel biomass such as cinnamon chips, rice husk firewood, sawdust pellets and fine coal."
        UuDiem = "Uses low-cost, high-moisture fuel to optimize steam generation costs by 30-40% compared with oil/gas boilers; low-temperature combustion (800-900C) reduces NOx emissions; fully automatic PLC-SCADA control for safe 24/7 operation."
        XuatXu = "Manufactured and installed by Phuc Truong Hai Co., Ltd."
        DevelopmentStage = "Commercialized"
    }
    35398 = @{
        Name = "KSRO4000/KSRO6000 Industrial RO Water Purifier (Rotek)"
        QueryString = "ksro4000-ksro6000-industrial-ro-water-purifier-rotek"
        MoTaNgan = "An industrial/commercial reverse osmosis (RO) water filtration system in the KSRO series, with 4000-6000 GPD flow rate."
        MoTa = 'The ROTEK KSRO series RO system is designed around the "Keep It Simple Stupid" principle, making it easy to install, operate and maintain thanks to a reliable "Pulse Flush" design. With a fixed recovery rate of 60-65%, it uses one ultra-high-flow RO membrane to produce 4000-6000 GPD (650-1000 LPH) of treated water.'
        ThongSo = "Flow rate: 4000-6000 GPD (650-1000 LPH); recovery rate: 60-65%; ROTEK XL-4040 membrane; patented ROTEK C-5TM RO controller; optional upgrade to Wi-Fi RO controller."
        UuDiem = "Simple one-switch operation; compact and easy to transport; highest treated-water flow in its segment; no conventional adjustment valve, reducing the risk of incorrect operation."
        XuatXu = "Taiwan (Rotek brand)"
    }
    35399 = @{
        Name = "Aobote Heat Pump Dryer for Fruits and Vegetables"
        QueryString = "aobote-heat-pump-dryer-for-fruits-and-vegetables"
        MoTaNgan = "A heat pump dryer for fruits and vegetables that saves electricity while preserving natural color and flavor."
        MoTa = "The Aobote dryer applies heat pump technology and operates in a closed-loop cycle: air is heated, passes through the material to remove moisture, water vapor is recovered and the air is reused. The system includes an insulated drying chamber, heat pump system, hot-air circulation fans, stainless steel drying trays and a smart PLC automatic control panel. It is widely used in agricultural processing, including sliced fruits (mango, banana, apple, pineapple), soft-dried fruits (jackfruit, dragon fruit) and vegetables (carrot, sweet potato, pumpkin)."
        ThongSo = "Drying technology: Heat Pump; main system: compressor, evaporator, condenser and expansion valve; automatic PLC control; multi-layer stainless steel trays; capacity and dimensions designed according to project requirements."
        UuDiem = "Saves electricity thanks to heat pump technology; preserves natural color and flavor; dries evenly; highly automated and easy to operate; suitable for many production scales from small workshops to industrial facilities."
        XuatXu = "China"
        DevelopmentStage = "Commercialized"
    }
    35455 = @{ Name = "IoT Controller for Water Filtration Equipment"; QueryString = "iot-controller-for-water-filtration-equipment"; MoTa = "A utility solution using IoT technology from Can Tho University, application No. 2-2019-00005, published in the Industrial Property Gazette. The smart controller supports remote monitoring and operation of water filtration equipment."; XuatXu = "Can Tho, Vietnam" }
    35456 = @{ Name = "Coconut Husk Rolling Machine"; QueryString = "coconut-husk-rolling-machine"; MoTa = "An agricultural processing mechanical invention from Can Tho University, application No. 1-2017-04153, published in the Industrial Property Gazette. The machine rolls and processes coconut husks for raw-material preprocessing."; XuatXu = "Can Tho, Vietnam" }
    35457 = @{ Name = "Chili Stem Removing Machine"; QueryString = "chili-stem-removing-machine"; MoTa = "An agricultural mechanical invention from Can Tho University, application No. 1-2019-04725, published in the Industrial Property Gazette of the Intellectual Property Office of Vietnam. The device automatically removes chili stems, reducing manual labor in agricultural preprocessing."; XuatXu = "Can Tho, Vietnam" }
    35458 = @{ Name = "Method for Simulating Cysteine Molecular Binding on a Silver Metal Surface"; QueryString = "method-for-simulating-cysteine-molecular-binding-on-a-silver-metal-surface"; MoTa = "Patent No. 54675, developed by Assoc. Prof. Dr. Nguyen Thanh Tien, Assoc. Prof. Dr. Pham Vu Nhat, Dr. Pham Thi Bich Thao (College of Natural Sciences) and Assoc. Prof. Dr. Dang Minh Triet (College of Education). The method simulates Cysteine molecular binding on a silver metal surface, opening application prospects in nanomaterials, computational chemistry and biotechnology."; XuatXu = "Can Tho, Vietnam" }
    35459 = @{ Name = "Method for Producing Giant Freshwater Prawn Seed"; QueryString = "method-for-producing-giant-freshwater-prawn-seed"; MoTa = "An invention by individual inventor Luong Thi Bao Thanh, application No. 1-2010-01458, published in the Industrial Property Gazette of the Intellectual Property Office of Vietnam. The method produces and nurses giant freshwater prawn seed for aquaculture in Can Tho."; XuatXu = "Can Tho, Vietnam" }
    35460 = @{ Name = "Process for Producing Gac-Carrot Juice"; QueryString = "process-for-producing-gac-carrot-juice"; MoTa = "A food technology invention from Can Tho University, application No. 1-2017-01652, published in the Industrial Property Gazette. The process produces juice combining gac fruit and carrot, utilizing local beta-carotene-rich raw materials."; XuatXu = "Can Tho, Vietnam" }
    35461 = @{ Name = "Super-Intensive Whiteleg Shrimp Farming Process in a Recirculating Multi-Species System"; QueryString = "super-intensive-whiteleg-shrimp-farming-process-in-a-recirculating-multi-species-system"; MoTa = "Utility Solution Patent No. 4743, developed by the research team of Prof. Dr. Tran Ngoc Hai (College of Aquaculture and Fisheries, Can Tho University). The process farms whiteleg shrimp (Litopenaeus vannamei) at super-intensive density in a recirculating water system combined with multi-species culture, improving production efficiency, optimizing resources and supporting sustainable shrimp farming in the Mekong Delta."; XuatXu = "Can Tho, Vietnam" }
    35462 = @{ Name = "Process for Treating Domestic Water Supply Using Cold Plasma"; QueryString = "process-for-treating-domestic-water-supply-using-cold-plasma"; MoTa = "An environmental technology invention from Can Tho University, application No. 1-2018-04189, published in the Industrial Property Gazette. The process applies cold plasma technology to treat and disinfect domestic water supply."; XuatXu = "Can Tho, Vietnam" }
    35463 = @{ Name = "Automatic Non-Contact Vehicle Dimension Measuring Device"; QueryString = "automatic-non-contact-vehicle-dimension-measuring-device"; MoTa = "An invention by individual inventor Chau Ngoc Y, application No. 1-2019-03118, published in the Industrial Property Gazette of the Intellectual Property Office of Vietnam. The device automatically measures vehicle dimensions using a non-contact method, with applications in inspection and traffic monitoring."; XuatXu = "Can Tho, Vietnam" }
    35464 = @{ Name = "Coconut Coir Fiber Stripping Device"; QueryString = "coconut-coir-fiber-stripping-device"; MoTa = "An agricultural processing mechanical invention from Can Tho University, application No. 1-2017-04842, published in the Industrial Property Gazette. The device strips coir fiber from coconut husks for the coconut-product processing industry."; XuatXu = "Can Tho, Vietnam" }
}

$supplierText = @{
    7225 = @{
        QueryString = "cong-ty-co-phan-machinex-viet-nam"
        ChucNangChinh = '<div id="pastingspan1">- Provides quality-certified machinery documentation (machines have been inspected by relevant authorities and agencies).</div><div id="pastingspan1">- Diverse products suitable for different needs.</div><div id="pastingspan1">- Provides completely free consultation on technology and solutions.</div><div id="pastingspan1">- Advises and helps customers choose products suitable for their investment scale based on the criteria of necessity and sufficiency.</div><div id="pastingspan1">- Advises on brand selection, marketing and related legal procedures.</div><div id="pastingspan1">- Helps customers choose partners supplying auxiliary equipment such as raw materials, labels and packaging.</div><div id="pastingspan1">- Provides long-term, stable and prompt warranty, maintenance and after-sales service.</div><div id="pastingspan1">- Introduces customers to visit and meet other partners in the same industry.</div>'
        SanPham = "Provides technologies and equipment for beverage production, essential oil distillation, post-harvest food processing and environmental fields."
    }
    8887 = @{ QueryString = "cong-ty-tnhh-china-ecotek-viet-nam"; ChucVu = "Director"; ChucNangChinh = "Supplier of industrial RO water purifiers (Rotek brand, Taiwan) and industrial LED lights." }
    8892 = @{ QueryString = "cong-ty-tnhh-phuc-truong-hai"; ChucNangChinh = "Specializes in designing, manufacturing and installing industrial boilers, including biomass-fired fluidized bed boilers." }
    8900 = @{ QueryString = "truong-dai-hoc-can-tho"; ChucNangChinh = "A key multidisciplinary university in the Mekong Delta and the owner of many patents and utility solutions in Can Tho." }
    8901 = @{ QueryString = "tong-cong-ty-vien-thong-mobifone"; ChucNangChinh = "Provides digital technology, telecommunications and enterprise software solutions."; DichVu = "Digital solutions, digital content and digital infrastructure" }
    8904 = @{
        QueryString = "cong-ty-co-phan-cong-nghe-cat-sach-mekong"
        ChucVu = "Director"
        ChucNangChinh = "Researches, manufactures and applies technology for industrial-scale screening and washing of sea sand, hill and mountain sand, river and stream sand, dredged sediment from waterways, estuaries and seaports, and recovered sand mixed with field and garden soil for agricultural land improvement in arid areas. The company promotes rational use of sand resources, addresses shortages of sand and problems caused by dirty sand such as moisture absorption and difficulty meeting module standards for concrete mix design. It aims to become a reputable leading enterprise in Vietnam providing the best and most effective fine-aggregate screening and washing equipment solutions."
        SanPham = "Sand screening, washing and classification services for organizations in need. Key technology products and research results: system and method for screening, washing and classifying saline sand; sand washing equipment systems and production lines; process for washing fine aggregates for construction; clean sand products after screening, washing and quality improvement through technology application."
    }
}

$productSourceFields = @("Name","MoTaNgan","MoTa","ThongSo","UuDiem","GiaiThuong","Keywords","XuatXu","Khachhang","CoQuanChuTri","CoQuanChuQuan","LoaiDeTaiKhac","TransferMethodKhac","TargetCustomer","DevelopmentStage","CooperationGoal","CooperationType","GiaBanDuKien","ChiPhiPhatSinh","BaoHanhHoTro","ChungNhanKhacText","InvestmentGoalKhac","NDAContent")
$supplierSourceFields = @("ChucNangChinh","DichVu","SanPham","ChungNhan")

$conn = [System.Data.SqlClient.SqlConnection]::new($connectionString)
$conn.Open()
try {
    $createdSuppliers = 0
    foreach ($id in ($supplierText.Keys | Sort-Object)) {
        $source = Get-Row $conn "SELECT * FROM dbo.NhaCungUng WHERE CungUngId=@id AND ISNULL(LanguageId,1)=1" @{ "@id" = [int]$id }
        if ($null -eq $source) { continue }
        $overrides = @{
            LanguageId = 2
            OriginalId = [int]$id
            EnStale = $false
            StatusId = 3
            Created = [DateTime]::Now
            CreatedBy = "codex"
            QueryString = $supplierText[$id].QueryString
            SourceHash = New-Sha256Hash ($supplierSourceFields | ForEach-Object { [string]$source[$_] })
        }
        foreach ($k in $supplierText[$id].Keys) { $overrides[$k] = $supplierText[$id][$k] }
        $result = Copy-RowWithOverrides $conn "NhaCungUng" "CungUngId" ([int]$id) $overrides
        if ($result.Created) { $createdSuppliers++ }
    }

    $createdProducts = 0
    foreach ($id in ($productText.Keys | Sort-Object)) {
        $source = Get-Row $conn "SELECT * FROM dbo.SanPhamCNTB WHERE ID=@id AND ISNULL(LanguageId,1)=1" @{ "@id" = [int]$id }
        if ($null -eq $source) { continue }
        $overrides = @{
            LanguageId = 2
            OriginalId = [int]$id
            EnStale = $false
            StatusId = 3
            Created = [DateTime]::Now
            Creator = "codex"
            Modified = [DateTime]::Now
            Modifier = "codex"
            SourceHash = New-Sha256Hash ($productSourceFields | ForEach-Object { [string]$source[$_] })
        }
        foreach ($k in $productText[$id].Keys) { $overrides[$k] = $productText[$id][$k] }
        if (-not $overrides.ContainsKey("QueryString")) { $overrides.QueryString = New-Slug $overrides.Name }
        $result = Copy-RowWithOverrides $conn "SanPhamCNTB" "ID" ([int]$id) $overrides
        if ($result.Created) {
            $createdProducts++
            $cmd = $conn.CreateCommand()
            $cmd.CommandText = "INSERT INTO dbo.SanPhamCNTBCategory (SanPhamCNTBId, CatId) SELECT @newId, CatId FROM dbo.SanPhamCNTBCategory WHERE SanPhamCNTBId=@oldId AND NOT EXISTS (SELECT 1 FROM dbo.SanPhamCNTBCategory WHERE SanPhamCNTBId=@newId AND CatId=dbo.SanPhamCNTBCategory.CatId);"
            [void]$cmd.Parameters.AddWithValue("@newId", $result.Id)
            [void]$cmd.Parameters.AddWithValue("@oldId", [int]$id)
            [void]$cmd.ExecuteNonQuery()
        }
    }

    "Created supplier translations: $createdSuppliers"
    "Created product translations: $createdProducts"
}
finally {
    $conn.Close()
}
